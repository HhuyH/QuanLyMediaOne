﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace QuanLyLinhKienDIenTu
{


    public static class Connecting
    {

        public static string GetConnectionString()
        {
            // Trả về chuỗi kết nối từ tệp cấu hình khi đống gối app
            //return ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;

            // kết nỗi tỉnh
            return @"Data Source=DESKTOP-4AOOEVT\SQLEXPRESS;Initial Catalog=QuanLyMediaOne;Integrated Security=True;";

        }


    }

    

}
