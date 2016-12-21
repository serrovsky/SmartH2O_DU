using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Configuration;
using System.Globalization;

namespace SmartH2O_DU
{
    class Program
    {
        static String ipAddress = ConfigurationSettings.AppSettings["ipAddressMessagingChannel"];
        static String topics = ConfigurationSettings.AppSettings["topics"];
        static SensorNodeDll.SensorNodeDll dll;

        static MqttClient m_cClient = new MqttClient(ipAddress);
        static string[] m_strTopicsInfo = { topics };

        static void Main(string[] args)
        {

            dll = new SensorNodeDll.SensorNodeDll();

            connectToMessagingChannel();

            dll.Initialize(getDataFromSensor, 2000);
        }

        private static void connectToMessagingChannel()
        {
            try
            {
                m_cClient.Connect(Guid.NewGuid().ToString());

                if (!m_cClient.IsConnected)
                {
                    Console.WriteLine("Error connecting to message broker...");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                closeConnection();
            }
        }

        private static void getDataFromSensor(string message)
        {
            string signalMessage = null;

            try
            {
                String[] signal = message.Split(';');

                int signalId = Int32.Parse(signal[0]);
                string signalName = signal[1];
                string signalValue = signal[2];

                signalMessage = createXml(signalId, signalName, signalValue);

                if (signalMessage != null)
                {
                    sendDataSensor(signalMessage);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                closeConnection();
            }

        }

        private static void closeConnection()
        {
            if (m_cClient.IsConnected)
            {
                m_cClient.Unsubscribe(m_strTopicsInfo);
                m_cClient.Disconnect();
            }

            dll.Stop();

            Console.ReadKey();

            Environment.Exit(-1);
        }

        private static void sendDataSensor(string signalMessage)
        {

            if (m_cClient.IsConnected)
            {
                m_cClient.Publish(m_strTopicsInfo[0], Encoding.UTF8.GetBytes(signalMessage));
            }
        }

        private static string createXml(int signalId, string signalName, string signalValue)
        {
            XmlDocument dataSensor = new XmlDocument();

            DateTime currentDate = DateTime.Now;
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;
            int weekNumber = cal.GetWeekOfYear(currentDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);


            XmlElement signal = dataSensor.CreateElement("signal");
            signal.SetAttribute("parameterType", signalName);
            signal.SetAttribute("parameterId", signalId.ToString());

            XmlElement value = dataSensor.CreateElement("value");
            value.InnerText = signalValue;

            XmlElement date = dataSensor.CreateElement("date");
            XmlElement day = dataSensor.CreateElement("day");
            day.InnerText = currentDate.Day.ToString();
            XmlElement month = dataSensor.CreateElement("month");
            month.InnerText = currentDate.Month.ToString();
            XmlElement year = dataSensor.CreateElement("year");
            year.InnerText = currentDate.Year.ToString();
            XmlElement hour = dataSensor.CreateElement("hour");
            hour.InnerText = currentDate.Hour.ToString();
            XmlElement minute = dataSensor.CreateElement("minute");
            minute.InnerText = currentDate.Minute.ToString();
            XmlElement second = dataSensor.CreateElement("second");
            second.InnerText = currentDate.Second.ToString();
            XmlElement week = dataSensor.CreateElement("week");
            week.InnerText = weekNumber.ToString();


            signal.AppendChild(value);
            date.AppendChild(day);
            date.AppendChild(month);
            date.AppendChild(year);
            date.AppendChild(hour);
            date.AppendChild(minute);
            date.AppendChild(second);
            date.AppendChild(week);
            signal.AppendChild(date);

            dataSensor.AppendChild(signal);

            Console.WriteLine(dataSensor.OuterXml);

            return dataSensor.OuterXml;
        }
    }
}
