﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SubscriptionsProject.Models.ApiModels
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}