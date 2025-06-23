﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageView.Helper
{
    /// <summary>
    /// 提供图像处理相关的扩展方法
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// 获取图像指定位置的像素灰度值
        /// </summary>
        /// <param name="bitmapImage">图像</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <returns>包含alpha、red、green、blue分量的数组</returns>
        /// <exception cref="ArgumentException">当坐标超出图像范围时抛出</exception>
        public static int[] GetPixelGray(this BitmapImage bitmapImage, int x, int y)
        {
            int[] gray = new int[4];
            WriteableBitmap writeableBitmap = new WriteableBitmap(bitmapImage);

            // 获取像素数据
            int width = writeableBitmap.PixelWidth;
            int height = writeableBitmap.PixelHeight;
            if(x < 0 || x >= width || y < 0 || y >= height)
            {
                throw new ArgumentException("坐标超出范围");
            }
            int pixelIndex = 10 + 10 * width;
            byte[] pixels = new byte[4];
            writeableBitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixels, 4, 0);
            // 像素颜色以BGR格式存储（在32位格式中）
            byte blue = pixels[0];
            byte green = pixels[1];
            byte red = pixels[2];
            byte alpha = pixels[3];

            gray[0] = alpha;
            gray[1] = red;
            gray[2] = green;
            gray[3] = blue;

            return gray;
        }
        public static void Save(this BitmapImage bitmapImage,string path,string type,int compressionQuality = 100)
        {
            type = type.ToLower();
            BitmapEncoder encoder;
            switch(type)
            {
                case ".png":
                    encoder = new PngBitmapEncoder();
                    break;
                case ".jpeg":
                case ".jpg":
                    var jpeg = new JpegBitmapEncoder
                    {
                        QualityLevel = compressionQuality
                    };
                    encoder = jpeg;
                    break;
                case ".bmp":
                    encoder = new BmpBitmapEncoder();
                    break;
                default:
                    throw new NotSupportedException("不支持的图片格式");
            }
            WriteableBitmap writeableBitmap = new WriteableBitmap(bitmapImage);
            encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
            // 保存到文件
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }
    }
}