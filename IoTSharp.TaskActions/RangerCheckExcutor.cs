using System;
using System.ComponentModel;
using System.Threading.Tasks;
using IoTSharp.Extensions;

namespace IoTSharp.TaskActions
{
    [DisplayName("用于增加范围属性的执行器")]
    public class RangerCheckExcutor : TaskAction
    {
        public override Task<TaskActionOutput> ExecuteAsync(TaskActionInput _input)
        {
            var data = JsonNodeParser.ParseObject(_input.Input);
            var config = JsonObjectSerializer.Deserialize<ModelExecutorConfig>(_input.ExecutorConfig);

            var pointX = data.SelectByPath(config.PointX);
            var pointY = data.SelectByPath(config.PointY);
            if (pointX != null && pointY != null)
            {
                var point = new Point();
                point.X = pointX.GetDoubleValue();
                point.Y = pointY.GetDoubleValue();
                data["iswithinrange"] = PointInFeaces(point, config.Ranger);
            }
            else
            {
                data["iswithinrange"] = true;
            }

            return Task.FromResult(new TaskActionOutput()
            {
                ExecutionInfo = data.ToJsonString(JsonOptions.Default),
                ExecutionStatus = true,
                DynamicOutput = data.ToClrObject()
            });
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
            return cnt % 2 > 0;
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
