using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IoTSharp.TaskAction
{
    [DisplayName("用于增加范围属性的执行器")]
    public class   RangerCheckExcutor : ITaskAction
    {
        public override Task<TaskActionOutput> ExecuteAsync(TaskActionInput _input)
        {
            JObject o = JsonConvert.DeserializeObject(_input.Input) as JObject;
            var config = JsonConvert.DeserializeObject<ModelExecutorConfig>(_input.ExecutorConfig);

            if (o.SelectToken(config.PointX) != null && o.SelectToken(config.PointY) != null)
            {
                var point = new Point();
                point.X = o.SelectToken(config.PointX).Value<double>();
                point.Y = o.SelectToken(config.PointY).Value<double>();
                if (PointInFeaces(point, config.Ranger))
                {
                    o.Add("iswithinrange", true);
                    return Task.FromResult(new TaskActionOutput() { ExecutionInfo = o.ToString(), ExecutionStatus = true, DynamicOutput = o }); ;
                }
                else
                {
                    o.Add("iswithinrange", false);
                    return Task.FromResult(new TaskActionOutput() { ExecutionInfo = o.ToString(), ExecutionStatus = true, DynamicOutput = o }); ;
                }
            }
            else
            {
                o.Add("iswithinrange", true);
                return Task.FromResult(new TaskActionOutput() { ExecutionInfo = o.ToString(), ExecutionStatus = true, DynamicOutput = o }); ;
            }
        }

        public static bool PointInFeaces(Point pnt, Point[] pntlist)
        {
            if (pntlist == null)
            {
                return false;
            }
            int j = 0, cnt = 0;
            for (int i = 0; i < pntlist.Length; i++)
            {
                j = (i == pntlist.Length - 1) ? 0 : j + 1;
                if ((pntlist[i].Y != pntlist[j].Y) && (((pnt.Y >= pntlist[i].Y) && (pnt.Y < pntlist[j].Y)) || ((pnt.Y >= pntlist[j].Y) && (pnt.Y < pntlist[i].Y))) && (pnt.X < (pntlist[j].X - pntlist[i].X) * (pnt.Y - pntlist[i].Y) / (pntlist[j].Y - pntlist[i].Y) + pntlist[i].X)) cnt++;
            }
            return (cnt % 2 > 0) ? true : false;
        }
        private class ModelExecutorConfig
        {
         

            public string PointX { get; set; }

            public string PointY { get; set; }

            public Point[] Ranger { get; set; }
        }

        public class Point
        {
            public double X { get; set; }
            public double Y { get; set; }
        }

    }
}
