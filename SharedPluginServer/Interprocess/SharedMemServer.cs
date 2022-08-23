using System;
using SharedMemory;


namespace SharedPluginServer
{
    public class SharedMemServer:IDisposable
    {
        private SharedArray<byte> _sharedBuf; // 内存交换通道

        private bool _isOpen; // 是否正在打开

        public string Filename; // 文件名称

        private static readonly log4net.ILog log =
   log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


       
        // 初始化
        public void Init(int size,string filename)
        {
            _sharedBuf=new SharedArray<byte>(filename,size);
            _isOpen = true;
            Filename = filename;

        }
        
        // 连接
        public void Connect(string filename)
        {
            try
            {
                _sharedBuf = new SharedArray<byte>(filename);
                Filename = filename;
                _isOpen = true;
                log.Debug("Server connected:" + filename);
            }
            catch (Exception ex)
            {
                _isOpen = false;
            }
        }

        // 获取状态
        public bool GetIsOpen()
        {
            return _isOpen;
        }

        // 重新改变内存交换通道的大小
        public void Resize(int newSize)
        {


            if (_sharedBuf.Length != newSize)
            {
                _sharedBuf.Close();
                _sharedBuf = new SharedArray<byte>(Filename, newSize);
            }
        }

        // 写入数据
        public void WriteBytes(byte[] bytes)
        {
            if (_isOpen)
            {
                if (bytes.Length > _sharedBuf.Length)
                {
                    Resize(bytes.Length);
                }
                _sharedBuf.Write(bytes);
            }
        }

        // 关闭
        public void Dispose()
        {
            _isOpen = false;
            _sharedBuf.Close();
        }
    
        // 读取数据
        public byte[] ReadBytes()
        {
            byte[] ret = null;
            if(_isOpen)
            {
                ret = new byte[_sharedBuf.Count];
                _sharedBuf.CopyTo(ret);

                //_sharedBuf.
            }

            return ret;
        }
      

    }

 }
