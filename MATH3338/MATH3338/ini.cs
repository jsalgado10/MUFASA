using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MATH3338
{
	#region Example
	/// <summary>
	/// Summary description for ini.
	/// Example of how to use this ini Class.....
	/// *******************************************************
	/// READING FROM INI FILE
	/// ******************************************************
	/// ini Config;
	/// Config.Path = @"C:\Setup.ini";
	/// if (Config.Read("APP", "Title") == "?")
	/// {
	///		Logit.aLog("Could not read from INI File. Error Message is : " + Config.lastError)
	/// }
	/// else
	/// {
	///		do what ever it is that you need to do with the value here
	/// }
	/// 
	/// ************************************************************
	/// WRITING TO AN INI FILE
	/// ************************************************************
	/// 
	/// ini Config(@"C:\Setup.ini";
	/// Config.Write("APP", "Title");
	/// if ( Config.lastError.Substring(0,2) == "34")
	/// {
	///		Logit.aLog(Error Writing to the ini file. Error message: " + Config.LastError);
	/// }
	/// 
	/// </summary>
	/// 
	#endregion
	public class ini
	{

		#region Variables
		private string m_path;
		private string m_lastError = "";
		#endregion

		#region DLL import

		[DllImport("kernel32")]
		private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);


		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string section, int key,
		string va, [MarshalAs(UnmanagedType.LPArray)] byte[] retValue,
		int size, string filePath );

		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(int section, string key,
		string val, [MarshalAs(UnmanagedType.LPArray)] byte[] retValue,
		int size, string filePath);

		#endregion

		#region Properties

		public string Path
		{
			get
			{
				return m_path;
			}
			set
			{
				m_path = value;
			}
		}

		public string LastError
		{
			get
			{
				return m_lastError;
			}
		}
		#endregion

		#region Constructors
		public ini(string inPath)
		{
			m_path = inPath;
		}

		public ini()
		{
			//do nothing, but this constructor requires you to use the private member m_path as 
			//the path to your ini file.
		}
		#endregion

		#region Methods
		
		public void Write(string section, string key, string value)
		{
			try
			{
				WritePrivateProfileString(section, key, value, m_path);
			}
			catch (Exception e)
			{
				m_lastError = "34: " + e.Message;
			}
		}

		public string Read(string section, string key)
		{
			try
			{
				StringBuilder temp = new StringBuilder(255);
				int i = GetPrivateProfileString(section, key, "",temp,255, m_path);
				return temp.ToString();
			}
			catch (Exception e)
			{
				m_lastError = "35: " + e.Message;
				return "?";
			}
		}

		public void ClearLastError()
		{
			m_lastError = null;
		}
		#endregion


		#region Get All Section Names in an ini file

		public string[] GetSectionNames()
		{
			try
			{
				//sets the maxsize buffer to 500, if more 
				//is required then double the size each time
				for ( int maxsize = 500; true; maxsize*=2)
				{
					byte[] bytes = new byte[maxsize];
					int size = GetPrivateProfileString(0,"","",bytes,maxsize,m_path );

					//check the information is not bigger
					//than the allocated maxsize buffer - 2 bytes
					//if it is then skip over the next section
					//so that the maxsize buffer can be doubled
					if ( size < maxsize -2)
					{
						//converts the bytes value into an ascii char. this is one long string
						string selected = Encoding.ASCII.GetString( bytes, 0, size - (size >0 ? 1:0 ));
						//splits the long string into an array based on the "\0"
						//or null (newline) value and returns the value(s) in an array
						return selected.Split( new char[] {'\0'});
					}
				}
			}
			catch ( Exception ex )
			{
				string err = ex.Message;
				return null;
			}
		}

		#endregion


		#region Enumerate All Entries in a Section

		public string[] GetEntryNames(string section)
		{
			try
			{
				for ( int maxsize = 500; true; maxsize*=2)
				{
					//get teh entrykey information in bytes
					//and store them in the maxsize buffer
					//not that section header value has been passed
					byte[] bytes = new byte[maxsize];
					int size = GetPrivateProfileString(section,0,"",bytes,maxsize,m_path );

					//check the info so it is not bigger
					//than the allocated maxsize buffer -2
					//if it is, then skip the next section
					// so that the maxsize buffer can be doubled
					if ( size < maxsize -2 )
					{
						string entries = Encoding.ASCII.GetString( bytes,0, size - (size >0 ? 1:0 ) );
						//splits the long string into an array based o nthe "\0"
						//or null (newline) value and returns the value(s) in an array
						return entries.Split(new char[] {'\0'});
					}
				}
			}
			catch ( Exception ex )
			{
				string err = ex.Message;
				return null;
			}
		}

		#endregion

		#region Get Value from enumeration

		public object GetEntryValue( string section, string entry )
		{
			for ( int maxsize=250; true; maxsize *=2 )
			{
				StringBuilder result = new StringBuilder(maxsize);
				int size = GetPrivateProfileString(section,entry,"",result,maxsize,m_path);
				if ( size < maxsize -1 )
				{
					return result.ToString();
				}
			}
		}

		#endregion


        #region Delete Methods

        public void DeleteKey(string sectionName, string keyName)
        {
            if (sectionName != null)
            {
                if (keyName != null)
                {
                    Write(sectionName, keyName, null);
                }
            }
        }

        public void DeleteSection(string sectionname)
        {
            if (sectionname != null)
                Write(sectionname, null, null);
        }

        #endregion
	}
}
