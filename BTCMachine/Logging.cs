using System;
using System.IO;
using System.Text;
using System.Threading;

namespace BTCMachine
{
    public class Logging
    {
        private static ReaderWriterLockSlim read_write_lock_ = new ReaderWriterLockSlim();
        private string preffix_ = "main";

        public void SetLogPrefix(string preffix) => this.preffix_ = preffix;

        public void AppendErrLog(string message)
        {
            DateTime now = DateTime.Now;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(now.ToString("yyyy/MM/dd,HH:mm:ss.fff") + ":");
            stringBuilder.Append(message);
            Logging.read_write_lock_.EnterWriteLock();
            try
            {
                using (StreamWriter streamWriter = File.AppendText("log/" + this.preffix_ + "_errlog_" + now.ToString("yyyyMMdd") + ".log"))
                {
                    streamWriter.WriteLine(stringBuilder.ToString());
                    streamWriter.Close();
                }
            }
            finally
            {
                Logging.read_write_lock_.ExitWriteLock();
            }
        }

        public void AppendEventLog(string message, int log_number)
        {
            DateTime now = DateTime.Now;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(now.ToString("yyyy/MM/dd,HH:mm:ss.fff") + ":");
            stringBuilder.Append(message);
            Logging.read_write_lock_.EnterWriteLock();
            try
            {
                using (StreamWriter streamWriter = File.AppendText("log/event" + (object)log_number + "_" + now.ToString("yyyyMMdd") + ".log"))
                {
                    streamWriter.WriteLine(stringBuilder.ToString());
                    streamWriter.Close();
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                Logging.read_write_lock_.ExitWriteLock();
            }
        }

        public void AppendSimpleLog(string message)
        {
            Logging.read_write_lock_.EnterWriteLock();
            try
            {
                using (StreamWriter streamWriter = File.AppendText("log\\log_" + DateTime.Now.ToString("yyyyMMdd") + ".log"))
                {
                    streamWriter.WriteLine(message);
                    streamWriter.Close();
                }
            }
            finally
            {
                Logging.read_write_lock_.ExitWriteLock();
            }
        }

        public void AppendLog(string message)
        {
            DateTime now = DateTime.Now;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(now.ToString("yyyy/MM/dd,HH:mm:ss.fff") + ":");
            stringBuilder.Append(message);
            Logging.read_write_lock_.EnterWriteLock();
            try
            {
                using (StreamWriter streamWriter = File.AppendText("log\\" + this.preffix_ + "_log_" + now.ToString("yyyyMMdd") + ".log"))
                {
                    streamWriter.WriteLine(stringBuilder.ToString());
                    streamWriter.Close();
                }
            }
            finally
            {
                Logging.read_write_lock_.ExitWriteLock();
            }
        }

        public void AppendDebugLog(string message)
        {
            DateTime now = DateTime.Now;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(now.ToString("yyyy/MM/dd,HH:mm:ss.fff") + ":");
            stringBuilder.Append(message);
            Logging.read_write_lock_.EnterWriteLock();
            try
            {
                using (StreamWriter streamWriter = File.AppendText("log\\" + this.preffix_ + "_log_debug_" + now.ToString("yyyyMMdd") + ".log"))
                {
                    streamWriter.WriteLine(stringBuilder.ToString());
                    streamWriter.Close();
                }
            }
            finally
            {
                Logging.read_write_lock_.ExitWriteLock();
            }
        }

        public void AppendSignalLog(string first_broker, string second_broker, double profit)
        {
            DateTime now = DateTime.Now;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(now.ToString("yyyy/MM/dd HH:mm:ss.fff") + ",");
            stringBuilder.Append(profit);
            Logging.read_write_lock_.EnterWriteLock();
            try
            {
                using (StreamWriter streamWriter = File.AppendText("log\\" + first_broker + "_" + second_broker + now.ToString("yyyyMMdd") + ".csv"))
                {
                    streamWriter.WriteLine(stringBuilder.ToString());
                    streamWriter.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("AppendSignalLog excpetion " + ex.Message);
            }
            finally
            {
                Logging.read_write_lock_.ExitWriteLock();
            }
        }
    }
}
