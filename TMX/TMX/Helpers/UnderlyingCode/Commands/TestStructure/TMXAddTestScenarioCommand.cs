﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 12/19/2012
 * Time: 11:46 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace TMX
{
    using System;
    using System.Management.Automation;
    
    /// <summary>
    /// Description of TmxAddTestScenarioCommand.
    /// </summary>
    internal class TmxAddTestScenarioCommand : TmxCommand
    {
        internal TmxAddTestScenarioCommand(CommonCmdletBase cmdlet) : base (cmdlet)
        {
        }
        
        internal override void Execute()
        {
            var cmdlet = (AddScenarioCmdletBase)Cmdlet;
            
            bool result = TmxHelper.AddTestScenario(cmdlet);
            
            if (result) {
                
                cmdlet.WriteObject(
                    cmdlet,
                    TestData.CurrentTestScenario);
            } else {
                cmdlet.WriteError(
                    cmdlet,
                    "Couldn't add a test scenario",
                    "AddingTestScenario",
                    ErrorCategory.InvalidArgument,
                    true);
            }
        }
    }
}
