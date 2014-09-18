﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 9/10/2014
 * Time: 10:06 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Tmx.Server.Modules
{
    using System;
    using System.Collections.Generic;
	using Nancy;
	using Nancy.ModelBinding;
	using TMX.Interfaces.Server;
	using Tmx.Core;
    
    /// <summary>
    /// Description of TestDataModule.
    /// </summary>
    public class TestDataModule : NancyModule
    {
        public TestDataModule() : base(UrnList.TestData_Root)
        {
            Get[UrnList.TestData_CommonData] = _ => null != CommonData.Data ? Response.AsJson(CommonData.Data).WithStatusCode(HttpStatusCode.OK) : HttpStatusCode.NotFound;
            
            Post[UrnList.TestData_CommonData] = _ => {
                var commonDataItem = this.Bind<CommonDataItem>();
                try {
                    var existingDataItem = CommonData.Data[commonDataItem.Key];
                    if (null != existingDataItem)
                        CommonData.Data[commonDataItem.Key] = commonDataItem.Value;
                    return HttpStatusCode.Created;
                }
                catch {
                    try {
                        CommonData.Data.Add(commonDataItem.Key, commonDataItem.Value);
                        return HttpStatusCode.Created;
                    } catch {
                        return HttpStatusCode.ExpectationFailed;
                    }
                }
            };
        }
    }
}
