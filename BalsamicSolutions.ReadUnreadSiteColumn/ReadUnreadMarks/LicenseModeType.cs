// -----------------------------------------------------------------------------
//  Copyright 5/7/2014 (c) Balsamic Solutions, Inc. All rights reserved.
//  This code is licensed under the Microsoft Public License.
//  THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//  ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//  IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//  PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
// ----------------------------------------------------------------------------- 
  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BalsamicSolutions.ReadUnreadSiteColumn
{
	public enum LicenseModeType:int
	{
		Invalid = 0,
		Evaluation = 1,
		Free = 2,
		Professional = 3,
		ProfessionalFarm = 4,
		Enterprise = 5,
		EnterpriseFarm = 6,
		Cloud = 7,
		CloudFarm = 8
	}
}