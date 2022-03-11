using Castle.Components.DictionaryAdapter;
using EasyCaching.Core;
using IoTSharp.Data;
using IoTSharp.Dtos;
using IoTSharp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IoTSharp.Controllers
{
    /// <summary>
    /// 登录验证码
    /// </summary>
    [Route("api/[controller]/[action]")]
    public class CaptchaController : ControllerBase
    {
        private readonly Random random = new Random();
        private readonly IEasyCachingProvider _caching;
        private readonly IWebHostEnvironment _hostingEnvironment;
  
        public CaptchaController(IWebHostEnvironment hostingEnvironment, IEasyCachingProvider caching)
        {
            _hostingEnvironment = hostingEnvironment;
            _caching = caching;
        }
        /// <summary>
        /// 生成一个图形认证
        /// </summary>
        /// <param name="clientid"></param>
        /// <returns></returns>
        [HttpGet, AllowAnonymous]
        public ApiResult<ModelCaptcha> Index(string clientid)
        {
            var data = CreateImage();
            var list = _caching.Get<List<ModelCaptchaVertifyItem>>("Captcha").Value ?? new EditableList<ModelCaptchaVertifyItem>();
            if (list.Count > 10000)
            {
                list.RemoveAt(0);
            }
            if (list.Any(c => c.Clientid == clientid))
            {
                list.Remove(list.FirstOrDefault(c => c.Clientid == clientid));
            }
            list.Add(new ModelCaptchaVertifyItem { Clientid = clientid, Move = data.Xwidth });
            _caching.Set("Captcha", list, expiration: TimeSpan.FromMinutes(20));
            return new ApiResult<ModelCaptcha>(ApiCode.Success, "OK", data);
        }
        /// <summary>
        /// 校验图形认证
        /// </summary>
        /// <param name="clientid"></param>
        /// <param name="m"></param>
        /// <returns></returns>
        [HttpPost]
        public ApiResult<bool> Vertify(string clientid, [FromBody] ModelCaptchaVertify m)
        {
            var list = _caching.Get<List<ModelCaptchaVertifyItem>>("Captcha").Value ?? new EditableList<ModelCaptchaVertifyItem>();

            var item = list.SingleOrDefault(c => c.Clientid == clientid);

            if (item == null)
            {
                return new ApiResult<bool>(ApiCode.InValidData, "expirate data", false);
            }

            if (Math.Abs(item.Move - m.move) < 3)
            {
                list.Remove(item);
                return new ApiResult<bool>(ApiCode.Success, "OK", true);
            }
            else
            {
                return new ApiResult<bool>(ApiCode.InValidData, "incorrect move", false);
            }
        }

        private ModelCaptcha CreateImage()
        {
            using var buzzlefile = new MemoryStream(Properties.Resources.ResourceManager.GetObject("buzzle_template_png") as byte[]);
            using var orginfile = new MemoryStream(Properties.Resources.ResourceManager.GetObject($"slide{random.Next(1, 9)}_jpg") as byte[]);
            using var buzzlefilestream = new SKManagedStream(buzzlefile);
            using var orginfilestream = new SKManagedStream(orginfile);
            using var buzzle = SKBitmap.Decode(buzzlefilestream);
            using var original = SKBitmap.Decode(orginfilestream);
            int buzzleWidth = buzzle.Width;
            int buzzleHeight = buzzle.Height;
            int oriImageWidth = original.Width;
            int oriImageHeight = original.Height;
            int randomlocaltionx = random.Next(oriImageWidth - 2 * buzzleWidth) + buzzleWidth;
            int randomlocaltiony = random.Next(oriImageHeight - buzzleHeight);
            return Cut(original, buzzle, randomlocaltionx, randomlocaltiony, buzzleWidth, buzzleHeight);
        }

        private SKColor CaclAvg(SKColor[][] matrix)
        {
            int r = 0;
            int g = 0;
            int b = 0;
            for (int i = 0; i < matrix.Length; i++)
            {
                SKColor[] x = matrix[i];
                for (int j = 0; j < x.Length; j++)
                {
                    if (j == 1)
                    {
                        continue;
                    }
                    var c = x[j];
                    r += c.Red;
                    g += c.Green;
                    b += c.Blue;
                }
            }

            return new SKColor((byte)(r / 8), (byte)(g / 8), (byte)(b / 8));
        }

        private ModelCaptcha Cut(SKBitmap OriginImage, SKBitmap ImageTemplate, int X, int Y, int BuzzleWidth, int BuzzleHeight)
        {
            var imagedata = new SKBitmap(BuzzleWidth, BuzzleHeight);
            SKImage sourcegra = SKImage.FromBitmap(imagedata);
            SKColor[][] martrix = new SKColor[3][];
            martrix[0] = new SKColor[3];
            martrix[1] = new SKColor[3];
            martrix[2] = new SKColor[3];
            SKColor[] values = new SKColor[9];

            for (int i = 0; i < BuzzleWidth; i++)
            {
                for (int j = 0; j < BuzzleHeight; j++)
                {
                    var pixel = ImageTemplate.GetPixel(i, j);
                    var rgb = (pixel.Alpha << 24) | (pixel.Red << 16) | (pixel.Green << 8) | pixel.Blue;
                    if (rgb < 0)
                    {
                        imagedata.SetPixel(i, j, OriginImage.GetPixel(X + i, Y + j));
                        ReadPixel(OriginImage, X + i, Y + j, values);
                        FillPixel(martrix, values);
                        OriginImage.SetPixel(X + i, Y + j, CaclAvg(martrix));
                    }
                }
            }

            ModelCaptcha m = new ModelCaptcha();
            m.SmallImage = Convert.ToBase64String(imagedata.Encode(SKEncodedImageFormat.Png, 50).ToArray());
            m.BigImage = Convert.ToBase64String(OriginImage.Encode(SKEncodedImageFormat.Jpeg, 50).ToArray());
            m.Xwidth = X;
            m.Yheight = Y;
            return m;
        }

        private void FillPixel(SKColor[][] Target, SKColor[] Source)
        {
            int filled = 0;
            for (int i = 0; i < Target.Length; i++)
            {
                SKColor[] x = Target[i];
                for (int j = 0; j < x.Length; j++)
                {
                    x[j] = Source[filled++];
                }
            }
        }

        private void ReadPixel(SKBitmap img, int x, int y, SKColor[] pixels)
        {
            int xStart = x - 1;
            int yStart = y - 1;
            int current = 0;
            for (int i = xStart; i < 3 + xStart; i++)
                for (int j = yStart; j < 3 + yStart; j++)
                {
                    int tx = i;
                    if (tx < 0)
                    {
                        tx = -tx;
                    }
                    else if (tx >= img.Width)
                    {
                        tx = x;
                    }
                    int ty = j;
                    if (ty < 0)
                    {
                        ty = -ty;
                    }
                    else if (ty >= img.Height)
                    {
                        ty = y;
                    }
                    pixels[current++] = img.GetPixel(tx, ty);
                }
        }
    }
}