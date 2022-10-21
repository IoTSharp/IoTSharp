namespace IoTSharp.Dtos
{
    public class MenuItem
    {
        public string text { get; set; }
        public string i18n { get; set; }
        public string vi18n { get; set; }
        public string routename { get; set; }
        public bool group { get; set; }
        public bool hideInBreadcrumb { get; set; }
        public string vpath { get; set; }
        public bool isAffix { get; set; }
        public bool isLink { get; set; }
        public string vicon { get; set; }
        public string link { get; set; }
        public string icon { get; set; }
        public MenuItem[] children { get; set; }
    }
}
