namespace IoTSharp.Models
{
    public class ModelCaptcha
    {
        public int Yheight { get; set; }
        public int Xwidth { get; set; }

        public string SmallImage { get; set; }

        public string BigImage { get; set; }
    }
    public class ModelCaptchaVertify
    {

        public int move { get; set; }

        public int[] action { get; set; }
    }

    public class ModelCaptchaVertifyItem
    {
        public int Move { get; set; }
        public string Clientid { get; set; }

    }


}
