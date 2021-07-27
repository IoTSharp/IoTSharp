using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IoTSharp.Models.DeviceMapping.Prop;

namespace IoTSharp.Models.DeviceMapping
{
    public class Node<T>
    {
        public string id { get; set; }
        public T Biz { get; set; }  //原始设备数据
        public int zIndex { get; set; }
        public Point position { get; set; }
        public Size size { get; set; }
        public string shape { get; set; } //Shape类型，网关为矩形
        public Port ports { get; set; }  //对应端口数据

        public Attr attrs { get; set; }  //名称
    }
    public class Port
    {
        public Group groups { get; set; } //描述端口绘制属性
        public PortInfo items { get; set; }
    }

    public class PortInfo
    {
        public string id { get; set; } //端口Id
        public string group { get; set; } //输入输出 ，in或者out
    }

    public class Group
    {
        public GroupItem @in { get; set; }  
        public GroupItem @out { get; set; }
    }
    public class GroupItem
    {
        public Position position { get; set; }
        public Attr attrs { get; set; }
        public Label label
        { get; set; }
    }

 
}
