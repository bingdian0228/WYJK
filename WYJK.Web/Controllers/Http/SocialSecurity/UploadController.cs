using WYJK.Entity;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Jobs.Web;

namespace WYJK.Web.Controllers
{
    /// <summary>
    /// 文件上传接口
    /// </summary>
    public class UploadController : BaseApiController
    {
        private static readonly string BasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UploadFiles");
        private static readonly string[] ImageExt = { ".jpg", ".jpeg", ".png", ".gif" };
        private static readonly string[] FileExt = { ".doc", ".docx", ".xml", ".xmlx", ".pdf", ".txt" };
        private static readonly int FileMaxSize = 4194304;//4MB
        private static readonly int MaxHeight = 260;
        private static readonly int Compression = 100;

        //#region 单文件JSON格式文件上传接口
        ///// <summary>
        ///// 单文件JSON格式文件上传接口
        ///// </summary>
        ///// <param name="entity">进行JSON序列化后的实体。</param>
        ///// <remarks>
        ///// 需要添加UserId请求头。如果图片需要压缩剪切，需要添加 Thumbnail=true 请求头。
        ///// </remarks>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<JsonResult<FileEntity>> UploadFileEntity(UploadFileEntity entity)
        //{
        //    #region 校验数据
        //    if (entity == null || string.IsNullOrWhiteSpace(entity.FileName) || entity.FileData == null ||
        //entity.FileData.Length <= 0)
        //    {
        //        return new JsonResult<FileEntity>(500, "请选择需要上传的文件");
        //    }
        //    int uid = Request.Headers.GetFirstOfDefault<int>("UserId");

        //    if (uid <= 0)
        //    {
        //        return new JsonResult<FileEntity>(500, "解析用户失败");
        //    }

        //    IUserService userService = new UserService();
        //    UserSimpleModel userModel = await userService.GetUserById(uid);
        //    if (userModel == null)
        //    {
        //        return new JsonResult<FileEntity>(500, "解析用户信息失败");
        //    }
        //    string ext = Path.GetExtension(entity.FileName);
        //    if (string.IsNullOrWhiteSpace(ext) ||
        //        (ImageExt.Any(et => et.Equals(ext, StringComparison.OrdinalIgnoreCase)) == false) &&
        //        FileExt.Any(et => et.Equals(ext, StringComparison.OrdinalIgnoreCase) == false))
        //    {
        //        return new JsonResult<FileEntity>(500, "不允许的文件类型");
        //    }
        //    if (entity.FileData.Length > FileMaxSize)
        //    {
        //        return new JsonResult<FileEntity>(500, "文件大小不能大于4M");
        //    }
        //    #endregion

        //    string fileName = uid + "M" + Guid.NewGuid().ToString("N") + ext;

        //    string filePath = Path.Combine(BasePath, DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString(), fileName);
        //    try
        //    {
        //        DirectoryInfo directory = new DirectoryInfo(Path.GetDirectoryName(filePath) ?? "");
        //        if (directory.Exists == false)
        //        {
        //            directory.Create();
        //        }
        //        byte[] fileBytes = Convert.FromBase64String(entity.FileData);

        //        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        //        {
        //            await fs.WriteAsync(fileBytes, 0, entity.FileData.Length);
        //        }
        //        if ("true".Equals(Request.Headers.GetFirstOfDefault("Thumbnail"), StringComparison.OrdinalIgnoreCase) &&
        //            ImageExt.Any(et => et.Equals(ext, StringComparison.InvariantCultureIgnoreCase)))
        //        {

        //            ZipBitmap(filePath);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error("保存文件失败", ex);
        //        return new JsonResult<FileEntity>(500, "保存文件失败");
        //    }
        //    FileEntity fileEntity = new FileEntity
        //    {
        //        Name = fileName,
        //        Path = filePath.Replace(AppDomain.CurrentDomain.BaseDirectory, WebConfigurationManager.ImageDomain).Replace("\\", "/")
        //    };

        //    return new JsonResult<FileEntity>
        //    {
        //        ErrorCode = 200,
        //        Message = "上传成功",
        //        Data = fileEntity
        //    };
        //}
        //#endregion

        //#region 使用文件流的方式单文件上传文件
        ///// <summary>
        ///// 使用文件流的方式上传文件
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<JsonResult<string>> Upload()
        //{
        //    string originalFileName = Request.Content.Headers.ContentDisposition.FileName.Replace("\"", "");
        //    byte[] fileBytes = await Request.Content.ReadAsByteArrayAsync();

        //    #region 校验数据
        //    if (string.IsNullOrWhiteSpace(originalFileName) || fileBytes == null || fileBytes.Length <= 0)
        //    {
        //        return new JsonResult<string>(false, "请选择需要上传的文件");
        //    }


        //    string ext = Path.GetExtension(originalFileName);
        //    if (string.IsNullOrWhiteSpace(ext) ||
        //        (ImageExt.Any(et => et.Equals(ext, StringComparison.OrdinalIgnoreCase)) == false) &&
        //        FileExt.Any(et => et.Equals(ext, StringComparison.OrdinalIgnoreCase) == false))
        //    {
        //        return new JsonResult<string>(false, "不允许的文件类型");
        //    }
        //    if (fileBytes.Length > FileMaxSize)
        //    {
        //        return new JsonResult<string>(false, "文件大小不能大于4M");
        //    }
        //    #endregion

        //    string fileName = Guid.NewGuid().ToString("N") + ext;

        //    string filePath = Path.Combine(BasePath, DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString(), fileName);
        //    try
        //    {
        //        DirectoryInfo directory = new DirectoryInfo(Path.GetDirectoryName(filePath) ?? "");
        //        if (directory.Exists == false)
        //        {
        //            directory.Create();
        //        }

        //        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        //        {
        //            await fs.WriteAsync(fileBytes, 0, fileBytes.Length);
        //        }
        //        //if ("true".Equals(Request.Headers.GetFirstOfDefault("Thumbnail"), StringComparison.OrdinalIgnoreCase) &&
        //        //    ImageExt.Any(et => et.Equals(ext, StringComparison.InvariantCultureIgnoreCase)))
        //        //{

        //        //    ZipBitmap(filePath);
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        //Logger.Error("保存文件失败", ex);
        //        return new JsonResult<string>(false, "保存文件失败");
        //    }
        //    //FileEntity fileEntity = new FileEntity
        //    //{
        //    //    Name = fileName,
        //    //    Path = filePath.Replace(AppDomain.CurrentDomain.BaseDirectory, WebConfigurationManager.ImageDomain).Replace("\\", "/")
        //    //};

        //    return new JsonResult<string>
        //    {
        //        status = true,
        //        Message = "上传成功",
        //        Data = filePath
        //    };
        //}
        //#endregion

        #region 实现多文件上传接口
        /// <summary>
        /// 实现多文件上传接口 需要添加MemberID请求头。如果图片需要压缩剪切，需要添加 Thumbnail=true 请求头。
        /// </summary>
        /// <remarks></remarks>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult<List<string>>> MultiUpload()
        {
            #region 数据校验
            if (Request.Content.IsMimeMultipartContent() == false)
            {
                //return new JsonResult<List<string>>(500, "接口只接受多文件上传");
            }
            int mid = Request.Headers.GetFirstOfDefault<int>("MemberID");

            //if (uid <= 0)
            //{
            //    return new JsonResult<List<FileEntity>>(500, "解析用户失败");
            //}

            //IUserService userService = new UserService();
            //UserSimpleModel userModel = await userService.GetUserById(uid);
            //if (userModel == null)
            //{
            //    return new JsonResult<List<FileEntity>>(500, "解析用户信息失败");
            //}
            #endregion

            MultipartMemoryStreamProvider streamProvider = new MultipartMemoryStreamProvider();

            await Request.Content.ReadAsMultipartAsync(streamProvider);
            List<string> fileListResult = new List<string>();
            foreach (var httpContent in streamProvider.Contents)
            {
                if (httpContent.Headers.ContentDisposition.FileName != null)
                {
                    string filename = httpContent.Headers.ContentDisposition.FileName.Replace("\"", "");
                    string ext = Path.GetExtension(filename);
                    if (string.IsNullOrWhiteSpace(ext) ||
               (ImageExt.Any(et => et.Equals(ext, StringComparison.OrdinalIgnoreCase)) == false) &&
               (FileExt.Any(et => et.Equals(ext, StringComparison.OrdinalIgnoreCase)) == false))
                    {
                        continue;
                    }
                    byte[] fileBytes = await httpContent.ReadAsByteArrayAsync();
                    if (fileBytes.Length > FileMaxSize)
                    {
                        continue;
                    }
                    string fileName = mid + "M" + Guid.NewGuid().ToString("N") + ext;

                    string filePath = Path.Combine(BasePath, DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString(), fileName);
                    DirectoryInfo directory = new DirectoryInfo(Path.GetDirectoryName(filePath) ?? "");
                    if (directory.Exists == false)
                    {
                        directory.Create();
                    }
                    try
                    {
                        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                        {
                            await fs.WriteAsync(fileBytes, 0, fileBytes.Length);
                        }
                        fileListResult.Add(filePath.Replace(BasePath, "UploadFiles").Replace("\\", "/"));
                        if ("true".Equals(Request.Headers.GetFirstOfDefault("Thumbnail"), StringComparison.OrdinalIgnoreCase) &&
                            ImageExt.Any(et => et.Equals(ext, StringComparison.InvariantCultureIgnoreCase)))
                        {

                            ZipBitmap(filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        return new JsonResult<List<string>>(false, "保存文件失败");
                    }
                }
            }
            return new JsonResult<List<string>>
            {
                status = true,
                Message = "处理完成",
                Data = fileListResult
            };
        }
        #endregion

        #region 获取图片缩略图
        ///// <summary>
        ///// 保存文件
        ///// </summary>
        ///// <param name="postedFile"></param>
        ///// <returns></returns>
        //public static async Task<string> UploadFile(HttpPostedFileBase postedFile)
        //{
        //    string ext = Path.GetExtension(postedFile.FileName);
        //    byte[] fileBytes = new byte[postedFile.ContentLength];
        //    await postedFile.InputStream.ReadAsync(fileBytes, 0, postedFile.ContentLength);

        //    string fileName = 0 + "M" + Guid.NewGuid().ToString("N") + ext;

        //    string filePath = Path.Combine(BasePath, DateTime.Now.Year.ToString(), DateTime.Now.Month.ToString(), fileName);
        //    try
        //    {
        //        DirectoryInfo directory = new DirectoryInfo(Path.GetDirectoryName(filePath) ?? "");
        //        if (directory.Exists == false)
        //        {
        //            directory.Create();
        //        }

        //        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        //        {
        //            await fs.WriteAsync(fileBytes, 0, fileBytes.Length);
        //        }
        //        ZipBitmap(filePath);

        //    }
        //    catch (Exception ex)
        //    {
        //        //Logger.Error("保存文件失败", ex);
        //        return string.Empty;
        //    }
        //    return filePath.Replace(AppDomain.CurrentDomain.BaseDirectory, WebConfigurationManager.ImageDomain).Replace("\\", "/");
        //}
        public static bool ZipBitmap(string sourcePath)
        {
            using (Bitmap bitmap = new Bitmap(sourcePath))
            using (Bitmap small = GetImageThumb(bitmap, MaxHeight))
            {
                ImageFormat format = bitmap.RawFormat;
                string ext = Path.GetExtension(sourcePath);

                string smallFilePath = sourcePath.Replace(ext ?? "", "_small" + ext);

                try
                {
                    ImageCodecInfo[] arrayIci = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo jpegIcIinfo = null;
                    for (int x = 0; x < arrayIci.Length; x++)
                    {
                        if (arrayIci[x].FormatDescription.Equals("JPEG", StringComparison.OrdinalIgnoreCase))
                        {
                            jpegIcIinfo = arrayIci[x];
                            break;
                        }
                    }
                    if (jpegIcIinfo != null)
                    {
                        EncoderParameters parameters = new EncoderParameters
                        {
                            Param = { [0] = new EncoderParameter(Encoder.Quality, new long[] { Compression }) }
                        };
                        small.Save(smallFilePath, jpegIcIinfo, parameters);
                    }
                    else
                    {
                        small.Save(smallFilePath, format);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    //Logger.Error("处理图片时出错",ex);
                }
            }
            return false;

        }
        /// <summary>
        /// 获取图片缩略图,高度固定，宽度等比例
        /// </summary>
        /// <param name="mg"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap GetImageThumb(Bitmap mg, int height)
        {
            if (mg.Height <= height)
            {
                return mg;
            }
            double scale = height / (double)mg.Height;

            Bitmap bp = new Bitmap(Convert.ToInt32(mg.Width * scale), height);

            using (Graphics g = Graphics.FromImage(bp))
            {
                g.SmoothingMode = SmoothingMode.HighSpeed;
                g.InterpolationMode = InterpolationMode.Low;
                g.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                Rectangle rect = new Rectangle(0, 0, bp.Width, bp.Height);
                g.DrawImage(mg, rect, 0, 0, mg.Width, mg.Height, GraphicsUnit.Pixel);
            }
            return bp;
        }
        #endregion
    }
}
