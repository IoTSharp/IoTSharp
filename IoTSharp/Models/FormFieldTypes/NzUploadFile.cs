using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Models.FormFieldTypes
{
    public class NzUploadFile
    {
        private string _FileName;
        private int _Uploaded;
        private string _Url;
        private string _Default;

        //保存的结构和设置的结构不一样 ，
        public string Name => fileName;

        public string fileName
        {
            get => _FileName;
            set => _FileName = value;
        }

        public int uploaded
        {
            get => _Uploaded;
            set => _Uploaded = value;
        }

        public string url
        {
            get => _Url;
            set => _Url = value;
        }

        public string @default
        {
            get => _Default;
            set => _Default = value;
        }
    }
}
