namespace blazorApp7.Data {
    public class GroupInfo {
        public string GroupId {get; set;} = null!;
        public string Name {get; set;} = null!;
        public string Note {get; set;} = null!;
    }
    public class GroupUserInfo : GroupInfo {
        public int Count {get; set;} = 0;
    }
}