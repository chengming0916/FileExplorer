﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public interface ILoginInfo
    {
        bool CheckLogin(Uri url);
        string StartUrl { get; }
    }

}
