using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ChatFunctions;
using System.Data.SqlClient;
using blazorApp7.Data;
using System.Linq;
using System.Threading;

namespace blazorApp7.Hubs {
    public class TestHub : Hub<IChatClientFunctions>, IChatHubFunctions {
        private readonly ILogger<TestHub> _logger;
        private string[] Colors = {
            "red","blue","orange","green","magenta","darkred",
            "darkblue","darkorange","darkgreen","maroon",
            "purple","olive" ,"teal","navy"
        };
        /// <summary>
        /// グループ毎の色使用状況
        ///     キーはグループID
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string,List<ColorUse>> ColorState = new Dictionary<string, List<ColorUse>>();
        /// <summary>
        /// 接続ユーザー一覧
        /// </summary>
        /// <typeparam name="string">接続ID</typeparam>
        /// <typeparam name="UserInfo"></typeparam>
        /// <returns></returns>
        private static Dictionary<string,UserInfo> Users = new Dictionary<string, UserInfo>();
        /// <summary>
        /// グループ一覧
        /// </summary>
        /// <typeparam name="string">グループID</typeparam>
        /// <typeparam name="GroupInfo"></typeparam>
        /// <returns></returns>
        private static Dictionary<string,GroupInfo> GroupInfos = new Dictionary<string, GroupInfo>();
        //private static Timer HubTimer = null;
        /// <summary>
        /// Hub Context保存用
        /// </summary>
        private static IHubContext<TestHub> _context = null!;
        public TestHub(ILogger<TestHub> logger,IHubContext<TestHub> ctx, IConfiguration conf) {
            _logger = logger;
            _context = ctx;
            // グループのロード
            if (GroupInfos?.Count == 0) {
                GroupInfos ??= new Dictionary<string, GroupInfo>();
                using(SqlConnection con = new SqlConnection()) {
                    string strCon = conf.GetConnectionString("ChatDBConnection")!;
                    con.ConnectionString = strCon;
                    con.Open();
                    SqlCommand cmd = new SqlCommand("select * from GroupDef",con);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    _logger.LogInformation($"GroupInfos={GroupInfos}");
                    while(rdr.Read()) {
                        _logger.LogInformation($"{rdr[0].ToString()},{rdr[1].ToString()},{rdr[2].ToString()}");
                        GroupInfos.Add(rdr[0].ToString()!, new GroupInfo() { GroupId=rdr[0].ToString()!, Name=rdr[1].ToString()!, Note=rdr[2].ToString()! });
                        // 色の使用状況を初期化
                        List<ColorUse> clst = new List<ColorUse>();
                        foreach(var c in Colors) {
                            clst.Add(new ColorUse() { Color = c });
                        }
                        // グループの使用色情報をセット
                        ColorState.Add(rdr[0].ToString()!,clst);
                    }
                    rdr.Close();
                }
            }
        }
        /// <summary>
        /// クライアント接続完了イベントハンドラ
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync() {
            await base.OnConnectedAsync();
#if NOTUSE            
            if (HubTimer == null) {
                _logger.LogInformation("Hub Timer has Created");
                HubTimer = new Timer(
                    async (e) => {
                        _logger.LogInformation("Hub Timer Fired");
                        await _context.Clients.All.SendAsync(nameof(IChatClientFunctions.FromServer),
                            $"Current Time={DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}");
                    },
                    null,10000,10000
                );
            }
#endif
        }
        /// <summary>
        /// SendMessageメッセージ受信時イベントハンドラ
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendMessage(string user, string message) {
            // 接続IDから選択グループの取得
            string groupid = Users[Context.ConnectionId].Group.GroupId;
            // グループに接続しているクライアント全てにメッセージを送信
            await Clients.Group(groupid).ReceiveMessage(
                user, message, Context.ConnectionId,Users[Context.ConnectionId].Color.Color);
            _logger.LogInformation($"Send Request from {Context.ConnectionId}:{user}");
        }
        /// <summary>
        /// グループを指定して色の空きを見つける
        /// </summary>
        /// <param name="groupid">グループID</param>
        /// <returns></returns>
        private ColorUse? GetColor(string groupid) {
            foreach(var ci in ColorState[groupid]) {
                if (!ci.Used) {
                    ci.Used = true;
                    return ci;
                }
            }
            return null;
        }
        /// <summary>
        /// LogOnメッセージ受信ハンドラ
        /// </summary>
        /// <param name="user">ユーザー名</param>
        /// <param name="groupid">グループID</param>
        /// <returns></returns>
        public async Task LogOn(string user,string groupid) {
            // ユーザー情報の追加
            Users.Add(Context.ConnectionId,new UserInfo() { Name=user, ConnectionID = Context.ConnectionId, Color=GetColor(groupid)!, Group=GroupInfos[groupid]});
            // ユーザーをグループに追加
            await Groups.AddToGroupAsync(Context.ConnectionId,groupid);
            // 所属グループメンバーにログインメッセージを送信
            await Clients.Group(groupid).LoggedOn(
                Context.ConnectionId,user,Users[Context.ConnectionId].Color.Color);
            _logger.LogInformation($"{nameof(IChatClientFunctions.LoggedOn)}({user}) Sent");
            _logger.LogInformation("Current Users");
        }
        /// <summary>
        /// LogOffメッセージ受信ハンドラ
        /// </summary>
        /// <param name="user">ユーザー名</param>
        /// <param name="groupid">グループID</param>
        /// <returns></returns>
        public async Task LogOff(string user,string groupid) {
            // 色を保存
            string color = Users[Context.ConnectionId].Color.Color;
            // 所属グループの色情報を更新
            Users[Context.ConnectionId].Color.Used = false;
            // ユーザー情報を削除
            Users.Remove(Context.ConnectionId);
            // グループからユーザーを削除
            await Groups.RemoveFromGroupAsync(Context.ConnectionId,groupid);
            // 所属グループメンバーへログオフメッセージ送信
            await Clients.Group(groupid).LoggedOff(
                Context.ConnectionId,user,color);
        }
        /// <summary>
        /// グループ一覧取得要求受信ハンドラ
        /// </summary>
        /// <returns></returns>
        public async Task GroupListRequest() {
            List<GroupInfo> lst = new List<GroupInfo>();
            foreach(var key in GroupInfos.Keys) {
                lst.Add(GroupInfos[key]);
            }
            await Clients.Caller.GroupListReply(lst);
        }
        /// <summary>
        /// クライアント接続解除受信時ハンドラ
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception? exception) {
            if (Users.ContainsKey(Context.ConnectionId)) {// 明示的に(ブラウザを閉じた等)ログオフしていなければ
                // 情報の保持
                string color = Users[Context.ConnectionId].Color.Color;
                string name = Users[Context.ConnectionId].Name;
                // 所属グループメンバーへログオフメッセージを送信
                await Clients.All.LoggedOff(
                    Context.ConnectionId,name,color);
                _logger.LogInformation("Client Disconnected LoggedOff sent");
                await base.OnDisconnectedAsync(exception);
                // ユーザー情報を削除
                Users[Context.ConnectionId].Color.Used = false;
                Users.Remove(Context.ConnectionId);
            }
        }
        /// <summary>
        /// ユーザー一覧取得要求ハンドラ
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns></returns>
        public async Task UserListRequest(string groupid) {
            List<string> lst = new List<string>();
            var q = Users.Where(u=>u.Value.Group.GroupId==groupid);
            foreach(var v in q) {
                lst.Add(v.Value.Name);
            }
            await Clients.Caller.UserListReply(lst);
        }
        /// <summary>
        /// グループ接続人数取得一覧要求ハンドラ
        /// </summary>
        /// <returns></returns>
        public async Task GroupStatRequest() {
            List<GroupUserInfo> lst = new List<GroupUserInfo>();
            foreach(var key in GroupInfos.Keys) {
                int cnt = Users.Where(k=>k.Value.Group.GroupId==key).Count();
                lst.Add(new GroupUserInfo() { GroupId = key, Name = GroupInfos[key].Name, Count = cnt});
            }
            await Clients.Caller.GroupStatReply(lst);
        }
    }
    /// <summary>
    /// ユーザー情報
    /// </summary>
    public class UserInfo {
        public string Name { get; set; } = null!;    // ユーザー(ハンドル)名
        public string ConnectionID {get; set;} = null!; // 接続ID
        public ColorUse Color {get; set;} = null!;  // 色使用情報⇒ここにはColorStateに設定されているインスタンスがセットされる
        public GroupInfo Group {get; set;} = null!; // グループ情報⇒ここにはGroupInfosに設定されているインスタンスがセットされる
    }
    /// <summary>
    /// 色使用情報
    /// </summary>
    public class ColorUse {
        public string Color {get; set;} = null!; // 色
        public bool Used {get; set;} = false;   // 使用/未使用
    }
}