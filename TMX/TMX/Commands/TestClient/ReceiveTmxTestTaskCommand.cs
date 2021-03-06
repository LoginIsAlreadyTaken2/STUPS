﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 7/17/2014
 * Time: 7:29 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Tmx.Commands
{
    using System;
    using System.Management.Automation;
    using Tmx;
    using Tmx.Client;
    
    /// <summary>
    /// Description of ReceiveTmxTestTaskCommand.
    /// </summary>
    [Cmdlet(VerbsCommunications.Receive, "TmxTestTask", DefaultParameterSetName = "LimitedTime")]
    public class ReceiveTmxTestTaskCommand : ClientCmdletBase
    {
        public ReceiveTmxTestTaskCommand()
        {
            this.Seconds = Preferences.ReceivingTaskTimeoutSeconds;
        }
        
        [Parameter(Mandatory = false,
                   ParameterSetName = "LimitedTime")]
        public int Seconds { get; set; }
        
        [Parameter(Mandatory = false,
                   ParameterSetName = "UnlimitedTime")]
        public SwitchParameter Continuous { get; set; }
        
        protected override void BeginProcessing()
        {
            var command = new ReceiveTestTaskCommand(this);
            command.Execute();
        }
        
        protected override void StopProcessing()
        {
            ClientSettings.Instance.StopImmediately = true;
        }
    }
}
