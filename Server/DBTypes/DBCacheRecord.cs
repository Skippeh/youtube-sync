using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DBTypes
{
	public class DBCacheRecord
	{
		public int ID { get; set; }
		public string VideoID { get; set; }
		public string ApiInfo { get; set; }
		public DateTime Date { get; set; }
	}
}