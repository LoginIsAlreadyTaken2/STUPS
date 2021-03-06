﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 10/31/2014
 * Time: 1:02 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Tmx.Server.Logic.ObjectModel
{
    using System.Linq;
    using Core;
    using Objects;
    using Tmx.Interfaces.Remoting;

    /// <summary>
    /// Description of TestRunSelector.
    /// </summary>
    public class TestRunSelector
    {
        public ITestRun GetNextInRowTestRun()
        {
            // var testRunsThatPending = TestRunQueue.TestRuns.Where(testRun => TestRunStatuses.Pending == testRun.Status);
            /*
            var testRunsThatPending = TestRunQueue.TestRuns.Where(testRun => testRun.IsPending());
            return !testRunsThatPending.Any() ? null : testRunsThatPending.OrderBy(testRun => testRun.CreatedTime).First();
            */
            var testRunsThatPending = TestRunQueue.TestRuns.Where(testRun => testRun.IsPending());
            var testRunsThatPendingArray = testRunsThatPending as ITestRun[] ?? testRunsThatPending.ToArray();
            return !testRunsThatPendingArray.Any() ? null : testRunsThatPendingArray.OrderBy(testRun => testRun.CreatedTime).First();
        }
        
        public void CancelTestRun(ITestRun testRun)
        {
            TaskPool.TasksForClients.Where(task => task.TestRunId == testRun.Id && !task.IsFinished() && !task.IsActive()).ToList().ForEach(task => task.TaskStatus = TestTaskStatuses.Canceled);
            testRun.Status = TestRunStatuses.Cancelled;
            if (TaskPool.TasksForClients.Any(task => task.TestRunId == testRun.Id && task.IsActive())) {
                TaskPool.TasksForClients.Where(task => task.TestRunId == testRun.Id && task.IsActive()).ToList().ForEach(task => task.TaskStatus = TestTaskStatuses.Interrupted);
                testRun.Status = TestRunStatuses.Cancelling;
            }
            
            // disconnecting clients
            ClientsCollection.Clients.RemoveAll(client => client.TestRunId == testRun.Id);
            testRun.SetTimeTaken();
            RunNextInRowTestRun();
        }
        
        public void RunNextInRowTestRun()
        {
            var testRun = GetNextInRowTestRun();
            if (null == testRun) return;
            if (TestRunQueue.TestRuns.Any(tr => tr.IsActive() && tr.TestLabId == testRun.TestLabId)) return;
            testRun.SetStartTime();
            testRun.Status = TestRunStatuses.Running;
        }
    }
}
