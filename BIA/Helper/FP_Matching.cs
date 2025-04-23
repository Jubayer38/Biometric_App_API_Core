using SourceAFIS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace BIA.Helper
{
    public class FP_Matching
    {
        public bool MatchFingerprint(byte[] providedFingerprint, List<byte[]> storedFingerprints)
        {
            try
            {
                var providedTemplate = CreateFingerprintTemplate(providedFingerprint, 500);

                var matcher = new FingerprintMatcher(providedTemplate);

                double thresholdScore = 30.1;

                foreach (var storedFingerprint in storedFingerprints)
                {
                    var storedTemplate = CreateFingerprintTemplate(storedFingerprint, 500);

                    double score = matcher.Match(storedTemplate);

                    if (score >= thresholdScore)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during fingerprint matching: {ex.Message}");
                throw;
            }

            return false;
        }

        private (int width, int height) GetFingerprintDimensions(int dataLength)
        {
            var knownResolutions = new Dictionary<int, (int width, int height)>
                {
                    { 300 * 400, (300, 400) },  // 300x400 resolution
                    { 260 * 300, (260, 300) },  // 260x300 resolution
                    { 256 * 288, (256, 288) }   // Add other known resolutions if necessary
                };

            if (knownResolutions.TryGetValue(dataLength, out var dimensions))
            {
                return dimensions;
            }

            throw new ArgumentException("Unknown fingerprint data length; dimensions cannot be determined.");
        }

        private FingerprintTemplate CreateFingerprintTemplate(byte[] fingerprintData, int dpi)
        {
            try
            {
                var (originalWidth, originalHeight) = GetFingerprintDimensions(fingerprintData.Length);

                using (var grayscaleBitmap = ToGrayscale(fingerprintData, originalWidth, originalHeight, dpi))
                {
                    int width = grayscaleBitmap.Width;
                    int height = grayscaleBitmap.Height;
                    byte[] pixelData = new byte[width * height];

                    BitmapData bmpData = grayscaleBitmap.LockBits(
                        new Rectangle(0, 0, width, height),
                        ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                    int stride = bmpData.Stride;
                    byte[] rawData = new byte[stride * height];
                    Marshal.Copy(bmpData.Scan0, rawData, 0, rawData.Length);

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int index = y * stride + x * 3;
                            byte grayscaleValue = rawData[index];
                            pixelData[y * width + x] = grayscaleValue;
                        }
                    }

                    grayscaleBitmap.UnlockBits(bmpData);

                    var options = new FingerprintImageOptions { Dpi = dpi };
                    var fingerprintImage = new FingerprintImage(width, height, pixelData, options);

                    return new FingerprintTemplate(fingerprintImage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating fingerprint template: {ex.Message}");
                throw;
            }
        }

        public Bitmap ToGrayscale(byte[] mImageBuffer, int mImageWidth, int mImageHeight, int dpi)
        {
            try
            {
                if (mImageBuffer.Length != mImageWidth * mImageHeight)
                {
                    throw new ArgumentException("The image buffer size does not match the specified dimensions.");
                }

                Bitmap grayscaleBitmap = new Bitmap(mImageWidth, mImageHeight, PixelFormat.Format24bppRgb);

                BitmapData bmpData = grayscaleBitmap.LockBits(
                    new Rectangle(0, 0, mImageWidth, mImageHeight),
                    ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                int stride = bmpData.Stride;
                byte[] pixelData = new byte[stride * mImageHeight];

                for (int y = 0; y < mImageHeight; y++)
                {
                    for (int x = 0; x < mImageWidth; x++)
                    {
                        byte grayscaleValue = mImageBuffer[y * mImageWidth + x];
                        int index = y * stride + x * 3;
                        pixelData[index] = grayscaleValue;
                        pixelData[index + 1] = grayscaleValue;
                        pixelData[index + 2] = grayscaleValue;
                    }
                }

                Marshal.Copy(pixelData, 0, bmpData.Scan0, pixelData.Length);
                grayscaleBitmap.UnlockBits(bmpData);
                grayscaleBitmap.SetResolution(dpi, dpi);

                return grayscaleBitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting to grayscale: {ex.Message}");
                throw;
            }
        }
    }
}
