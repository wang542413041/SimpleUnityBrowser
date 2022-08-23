using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageLibrary;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SharedPluginServer.Interprocess
{
    public class SharedCommServer: SharedMemServer
    {
        private static readonly log4net.ILog log =
log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //EventPacket _lastPacket = null;

        // 待发送事件队列
        Queue<EventPacket> _packetsToSend;

        // 是否正在写数据
        bool _isWrite = false;

        // 创建共享内存通道
        public SharedCommServer(bool write):base()
        {
            _isWrite = write; // 标记写状态
            _packetsToSend = new Queue<EventPacket>(); // 创建事件队列
        }

        // 初始化通道
        public  void InitComm(int size, string filename)
        {
            base.Init(size, filename); // 初始化sharedMemory
            WriteStop(); // 
        }

        // 检查状态
        private bool CheckIfReady()
        {
            byte[] arr = ReadBytes();  // 读取内存
            if (arr != null) // 内存不为空
            {
                try // 尝试取内存
                {
                    MemoryStream mstr = new MemoryStream(arr); // 转化为MemoryStream流
                    BinaryFormatter bf = new BinaryFormatter(); // 转化为BinaryFormatter 流
                    EventPacket ep = bf.Deserialize(mstr) as EventPacket; // 转化为事件

                    if (ep.Type == BrowserEventType.StopPacket) // 判断事件状态
                        return true;
                    else
                        return false;
                }
                catch(Exception ex)
                {
                    return false;
                }
           }
            return false;
        }


        // 获取消息
        public EventPacket GetMessage()
        {
            if (_isWrite) // 如果是写，则返回空
                return null;

            byte[] arr = ReadBytes(); // 读取消息
          //  EventPacket ret = null;

            if(arr!=null) // 消息不为空
            {
                try
                {
                    MemoryStream mstr = new MemoryStream(arr);
                    BinaryFormatter bf = new BinaryFormatter();
                    EventPacket ep = bf.Deserialize(mstr) as EventPacket;

                    if(ep!=null&&ep.Type!=BrowserEventType.StopPacket)
                    {
                        //_lastPacket = ep;
                        //log.Info("_____RETURNING PACKET:" + ep.Type.ToString());
                        WriteStop(); // 停止写事件
                        return ep; // 反馈事件
                    }
                    else
                    {
                        
                        return null;
                    }
                }
                catch(Exception ex)
                {
                    log.Error("Serialization exception,length="+arr.Length+":" + ex.Message);
                    return null;
                }
            }
            return null;
        }

        // 停止写
        private void WriteStop()
        {
            EventPacket e = new EventPacket // 创建事件
            {
                Type = BrowserEventType.StopPacket
            };

            MemoryStream mstr = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(mstr, e);
            byte[] b = mstr.GetBuffer(); // 转化为buffer
            WriteBytes(b); // 写入内存
        }

        // 写入消息
        public void WriteMessage(EventPacket ep)
        {

            bool sent = false; // 是否已发送
            while(!sent) // 当状态为未发送
            {
                if(CheckIfReady()) // 检查状态
                {
                    MemoryStream mstr = new MemoryStream();
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(mstr, ep);
                    byte[] b = mstr.GetBuffer();
                    WriteBytes(b); // 吸入消息
                    sent = true; // 设置状态已发送
                }
            }
           /* if(_isWrite)
            {
                _packetsToSend.Enqueue(ep);
            }*/
        }

        // 推消息
        public void PushMessages()
        {
            if(_packetsToSend.Count!=0) // 如果消息队列不为空
            {
                if(CheckIfReady()) // 检查是否可以发送消息
                {
                    EventPacket ep = _packetsToSend.Dequeue(); // 获取最上层数据

                    MemoryStream mstr = new MemoryStream();
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(mstr, ep);
                    byte[] b = mstr.GetBuffer();
                    WriteBytes(b); // 包装并发送消息
                }
            }
        }

    }
}
