﻿/*
 * Created by SharpDevelop.
 * User: Alexander Petrovskiy
 * Date: 7/22/2014
 * Time: 3:37 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace Tmx.Server.Tests.Modules
{
    using System;
    using System.Linq;
    using Nancy;
    using Nancy.Testing;
    using MbUnit.Framework;
    using NUnit.Framework;
    using Interfaces.Server;
    using Client;
    using Core;
    using Core.Types.Remoting;
    using Interfaces.Remoting;
    using Logic.ObjectModel.Objects;
    using Xunit;
    using UnitTestingHelpers;
    using Xunit.Extensions;
    
    /// <summary>
    /// Description of TestTasksModuleTestFixture.
    /// </summary>
    [MbUnit.Framework.TestFixture][NUnit.Framework.TestFixture]
    public class TestTasksModuleTestFixture
    {
        const string _testClientHostnameExpected = "testhost";
        const string _testClientUsernameExpected = "aaa";
        ITestWorkflow _workflow;
        ITestRun _testRun;
        BrowserResponse response;
        Browser _browser;
        DateTime _startTime;
        
        public TestTasksModuleTestFixture()
        {
            TestSettings.PrepareModuleTests();
            _browser = TestFactory.GetBrowserForTestTasksModule();
            TestFactory.GetTestRunWithStatus(TestRunStatuses.Running);
            _workflow = WorkflowCollection.Workflows.First();
            _testRun = TestRunQueue.TestRuns.First();
            TestFactory.GetAnotherTestRunWithStatus(TestRunStatuses.Pending, _workflow);
        }
        
        [MbUnit.Framework.SetUp][NUnit.Framework.SetUp]
        public void SetUp()
        {
            TestSettings.PrepareModuleTests();
            _browser = TestFactory.GetBrowserForTestTasksModule();
            TestFactory.GetTestRunWithStatus(TestRunStatuses.Running);
            _workflow = WorkflowCollection.Workflows.First();
            _testRun = TestRunQueue.TestRuns.First();
            TestFactory.GetAnotherTestRunWithStatus(TestRunStatuses.Pending, _workflow);
        }
        
        [MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_provide_a_task_to_test_client_if_the_client_matches_the_rule()
        {
            var expectedTask = GIVEN_Loaded_TestTask(5, "task name", false, TestTaskStatuses.New, true, _testClientHostnameExpected, 0);
            var testClient = GIVEN_Registered_TestClient_as_json(_testClientHostnameExpected, _testClientUsernameExpected);
            
            var actualTask = WHEN_Getting_task_as_json(testClient.Id);
            
            THEN_HttpResponse_Is_Ok();
            THEN_TestTask_Properties_Equal_To(expectedTask, actualTask, TestTaskStatuses.Running);
            THEN_test_client_is_busy(ClientsCollection.Clients.First(client => client.Id == testClient.Id));
        }
        
        [MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_provide_no_task_to_test_client_if_the_client_does_not_match_the_rule()
        {
            var givenTask = GIVEN_Loaded_TestTask(5, "task name", false, TestTaskStatuses.New, true, "no matches", 0);
            var testClient = GIVEN_Registered_TestClient_as_json(_testClientHostnameExpected, _testClientUsernameExpected);
            
            WHEN_Getting_task_as_json(testClient.Id);
            
            THEN_HttpResponse_Is_NotFound();
            THEN_test_client_is_free(testClient);
        }
        
        [MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_provide_the_second_task_if_the_client_matches_the_rule()
        {
            var givenTask01 = GIVEN_Loaded_TestTask(1, "task name", false, TestTaskStatuses.New, true, ".*h.*", 0);
            var givenTask02 = GIVEN_Loaded_TestTask(2, "task name 02", false, TestTaskStatuses.New, true, "u", 0);
            var registeredClient = GIVEN_Registered_TestClient_as_json("h", "u");
            
            var actualTask = WHEN_Getting_task_as_json(registeredClient.Id);
            WHEN_Finishing_Task_as_json(actualTask);
            actualTask = WHEN_Getting_task_as_json(registeredClient.Id);
            
            THEN_HttpResponse_Is_Ok();
            THEN_TestTask_Properties_Equal_To(givenTask02, actualTask, TestTaskStatuses.Running);
            THEN_test_client_is_busy(ClientsCollection.Clients.First(client => client.Id == registeredClient.Id));
        }
        
        [MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_not_provide_the_second_task_if_the_client_does_not_match_the_rule()
        {
            var givenTask01 = GIVEN_Loaded_TestTask(1, "task name", false, TestTaskStatuses.New, true, ".*h.*", 0);
            var givenTask02 = GIVEN_Loaded_TestTask(2, "task name 02", false, TestTaskStatuses.New, true, "aaa", 0);
            var registeredClient = GIVEN_Registered_TestClient_as_json("h", "u");
            
            var actualTask = WHEN_Getting_task_as_json(registeredClient.Id);
            WHEN_Finishing_Task_as_json(actualTask);
            WHEN_Getting_task_as_json(registeredClient.Id);
            
            THEN_HttpResponse_Is_NotFound();
        }
        
        [MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_provide_the_second_task_if_the_client_matches_the_rule_and_there_are_several()
        {
            var givenTask01 = GIVEN_Loaded_TestTask(1, "task name", false, TestTaskStatuses.New, true, ".*h.*", 0);
            var givenTask02 = GIVEN_Loaded_TestTask(2, "task name", false, TestTaskStatuses.New, true, ".*aaa.*", 0);
            var givenTask03 = GIVEN_Loaded_TestTask(3, "task name 02", false, TestTaskStatuses.New, true, "u", 0);
            var givenTask04 = GIVEN_Loaded_TestTask(4, "task name", false, TestTaskStatuses.New, true, ".*aaa.*", 0);
            var registeredClient = GIVEN_Registered_TestClient_as_json("h", "u");
            
            var actualTask = WHEN_Getting_task_as_json(registeredClient.Id);
            WHEN_Finishing_Task_as_json(actualTask);
            actualTask = WHEN_Getting_task_as_json(registeredClient.Id);
            
            THEN_HttpResponse_Is_Ok();
            THEN_TestTask_Properties_Equal_To(givenTask03, actualTask, TestTaskStatuses.Running);
            Xunit.Assert.Equal(givenTask03.Id, TaskPool.TasksForClients.OrderBy(t => t.Id).Skip(1).First().Id);
            THEN_test_client_is_busy(ClientsCollection.Clients.First(client => client.Id == registeredClient.Id));
        }
        
        [MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_not_provide_the_second_task_if_the_client_does_not_match_the_rule_and_there_are_several()
        {
            var givenTask01 = GIVEN_Loaded_TestTask(1, "task name", false, TestTaskStatuses.New, true, ".*h.*", 0);
            var givenTask02 = GIVEN_Loaded_TestTask(2, "task name", false, TestTaskStatuses.New, true, ".*aaa.*", 0);
            var givenTask03 = GIVEN_Loaded_TestTask(3, "task name 02", false, TestTaskStatuses.New, true, "aaa", 0);
            var givenTask04 = GIVEN_Loaded_TestTask(4, "task name 02", false, TestTaskStatuses.New, true, "aaa", 0);
            var registeredClient = GIVEN_Registered_TestClient_as_json("h", "u");
            
            var task = WHEN_Getting_task_as_json(registeredClient.Id);
            WHEN_Finishing_Task_as_json(task);
            WHEN_Getting_task_as_json(registeredClient.Id);
            
            THEN_HttpResponse_Is_NotFound();
        }
        
        [MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_cancel_all_further_tasks_on_fail()
        {
            var givenTask01 = GIVEN_Loaded_TestTask(1, "task name", false, TestTaskStatuses.New, true, ".*h.*", 0);
            var givenTask02 = GIVEN_Loaded_TestTask(2, "task name", false, TestTaskStatuses.New, true, ".*aaa.*", 0);
            var givenTask03 = GIVEN_Loaded_TestTask(3, "task name 02", false, TestTaskStatuses.New, true, "u", 0);
            var givenTask04 = GIVEN_Loaded_TestTask(4, "task name", false, TestTaskStatuses.New, true, ".*aaa.*", 0);
            var givenTask05 = GIVEN_Loaded_TestTask(5, "task name", false, TestTaskStatuses.New, true, "h", 0);
            var registeredClient = GIVEN_Registered_TestClient_as_json("h", "u");
            
            var actualTask = WHEN_Getting_task_as_json(registeredClient.Id);
            WHEN_Failing_Task_as_json(actualTask);
            actualTask = WHEN_Getting_task_as_json(registeredClient.Id);
            
            THEN_HttpResponse_Is_NotFound();
            Xunit.Assert.Equal(null, actualTask);
            Xunit.Assert.Equal(0, TaskPool.TasksForClients.Count(task => !task.IsFailed() && !task.IsCancelled()));
            Xunit.Assert.Equal(givenTask03.Id, TaskPool.TasksForClients.OrderBy(t => t.Id).Skip(1).First().Id);
            THEN_testRun_isCompleted();
        }
        
//[NUnit.Framework.Test, TestCaseSource("DivideCases")]
//public void DivideTest(int n, int d, int q)
//{
//    NUnit.Framework.Assert.AreEqual( q, n / d );
//}
//
//static object[] DivideCases =
//{
//    new object[] { 12, 3, 4 },
//    new object[] { 12, 5, 6 },
//    new object[] { 12, 2, 6 },
//    new object[] { 12, 4, 3 } 
//};
        
        [MbUnit.Framework.Test][NUnit.Framework.Test] // [Fact]
        [Xunit.Extensions.Theory]
        [InlineData("testHost001", "user001")]
        [InlineData("testHost002", "user002")]
        [InlineData("testHost003", "user003")]
//        [TestCase("testHost001", "user000")]
//        [TestCase("testHost002", "user002")]
//        [TestCase("testHost003", "user003")]
        // public void Should_not_provide_a_task_before_task_this_depends_on_is_completed() //string hostname, string username)
        public void Should_not_provide_a_task_before_task_this_depends_on_is_completed<T1, T2>(T1 hostname, T2 username)
        {
            string testClientHostnameExpected = hostname.ToString();
            string testClientUsernameExpected = username.ToString();
//            if ("user003" == username.ToString())
//                // Xunit.Assert.Equal(1, 2);
//                NUnit.Framework.Assert.AreEqual(1, 2);
//            else
//                NUnit.Framework.Assert.AreEqual(2, 3);
            
            var givenTask01 = GIVEN_Loaded_TestTask(4, "task name", false, TestTaskStatuses.New, true, "another rule", 0);
            var givenTask02 = GIVEN_Loaded_TestTask(5, "task name", false, TestTaskStatuses.New, true, testClientHostnameExpected, 4);
            var registeredClient = GIVEN_Registered_TestClient_as_json(testClientHostnameExpected, testClientUsernameExpected);
            
            var actualTask = WHEN_Getting_task_as_json(registeredClient.Id);
            
            THEN_TestTask_Is_Null(actualTask);
            THEN_test_client_is_free(registeredClient);
        }
        
        [MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_not_provide_a_task_before_task_this_depends_on_is_allocated()
        {
            var givenTask = GIVEN_Loaded_TestTask(5, "task name", false, TestTaskStatuses.New, true, _testClientHostnameExpected, 4);
            var registeredClient = GIVEN_Registered_TestClient_as_json(_testClientHostnameExpected, _testClientUsernameExpected);
            
            var actualTask = WHEN_Getting_task_as_json(registeredClient.Id);
            
            THEN_TestTask_Is_Null(actualTask);
            THEN_test_client_is_free(registeredClient);
        }
        
        [MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_provide_a_task_only_after_task_this_depends_on_is_completed()
        {
            var givenTask01 = GIVEN_Allocated_TestTask(4, "task name", true, TestTaskStatuses.CompletedSuccessfully, true, "another rule", 0);
            var givenTask02 = GIVEN_Loaded_TestTask(5, "task name", false, TestTaskStatuses.New, true, _testClientHostnameExpected, 4);
            var registeredClient = GIVEN_Registered_TestClient_as_json(_testClientHostnameExpected, _testClientUsernameExpected);
            
            var actualTask = WHEN_Getting_task_as_json(registeredClient.Id);
            
            THEN_HttpResponse_Is_Ok();
            THEN_TestTask_Properties_Equal_To(givenTask02, actualTask, TestTaskStatuses.Running);
            THEN_test_client_is_busy(ClientsCollection.Clients.First(client => client.Id == registeredClient.Id));
        }
        
        [MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_provide_no_task_to_unregistered_test_client()
        {
            var givenTask01 = GIVEN_Loaded_TestTask(5, "task name", false, TestTaskStatuses.New, true, _testClientHostnameExpected, 0);
            var registeredClient = GIVEN_Registered_TestClient_as_json(_testClientHostnameExpected, _testClientUsernameExpected);
            
            WHEN_SendingDeregistration_as_json(registeredClient);
            WHEN_Getting_task_as_json(Guid.Empty);
            
            THEN_HttpResponse_Is_ExpectationFailed();
        }
        
        [MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_provide_no_task_to_test_client_that_lost_its_registration()
        {
            var givenTask01 = GIVEN_Loaded_TestTask(5, "task name", false, TestTaskStatuses.New, true, _testClientHostnameExpected, 0);
            var registeredClient = GIVEN_Registered_TestClient_as_json(_testClientHostnameExpected, _testClientUsernameExpected);
            
            WHEN_Removing_Registered_Client_On_Server(registeredClient);
            WHEN_Getting_task_as_json(registeredClient.Id);
            if (HttpStatusCode.ExpectationFailed == response.StatusCode)
                registeredClient = GIVEN_Registered_TestClient_as_json(_testClientHostnameExpected, _testClientUsernameExpected);
            var actualTask = WHEN_Getting_task_as_json(registeredClient.Id);
            
            THEN_HttpResponse_Is_Ok(); //response);
            THEN_TestTask_Properties_Equal_To(givenTask01, actualTask, TestTaskStatuses.Running);
            Xunit.Assert.Equal(givenTask01.Id, TaskPool.TasksForClients.OrderBy(t => t.Id).Skip(1).First().Id);
            THEN_test_client_is_busy(ClientsCollection.Clients.First(client => client.Id == registeredClient.Id));
        }
        
        [MbUnit.Framework.Ignore][NUnit.Framework.Ignore]
        [MbUnit.Framework.Test][NUnit.Framework.Test]// [Fact]
        public void Should_complete_the_current_task_on_client_unregistration()
        {
            // TODO: do it!
//            // Given
//            var testClient = new TestClient { Hostname = testClientHostnameExpected, Username = testClientUsernameExpected };
//            var task = new TestTask {
//                Id = 5,
//                Name = "task name",
//                TaskFinished = false,
//                IsActive = true,
//                TaskStatus = TestTaskStatuses.New,
//                Rule = "no matches"
//            };
//            TaskPool.Tasks.Add(task);
//            
//            // When
//            var response = browser.Post(UrnList.TestClientRegistrationPoint, with => with.JsonBody<ITestClient>(testClient));
//            testClient = response.Body.DeserializeJson<TestClient>();
//            WHEN_SendingDeregistration(testClient);
//            response = browser.Get(UrnList.TestTasks_Root + "/" + 0);
//            
//            // Then
//            THEN_HttpResponse_Is_NotFound(response);
        }
        
        [MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_provide_task_by_task_on_loading_new_tasks()
        {
            var givenTask01 = GIVEN_Loaded_TestTask(5, "task name", false, TestTaskStatuses.New, true, _testClientHostnameExpected, 0);
            var registeredClient = GIVEN_Registered_TestClient_as_json(_testClientHostnameExpected, _testClientUsernameExpected);
            
            // the first task
            var actualTask = WHEN_Getting_task_as_json(registeredClient.Id);
            actualTask.TaskResult.Add("result01", "res01");
            actualTask.TaskResult.Add("result02", "res02");
            WHEN_Finishing_Task_as_json(actualTask);
            
            // the second task
            var givenTask02 = GIVEN_Loaded_TestTask(10, "task name", false, TestTaskStatuses.New, true, _testClientHostnameExpected, 0);
            givenTask02.ClientId = registeredClient.Id;
            TaskPool.TasksForClients.Add(givenTask02);
            actualTask = WHEN_Getting_task_as_json(registeredClient.Id);
            actualTask.TaskResult.Add("result01", "res01");
            actualTask.TaskResult.Add("result02", "res02");
            WHEN_Finishing_Task_as_json(actualTask);
            
            THEN_HttpResponse_Is_Ok();
            THEN_TestTask_Properties_Equal_To(givenTask02, actualTask);
            Xunit.Assert.Equal(givenTask02.Id, TaskPool.TasksForClients.OrderBy(t => t.Id).Skip(1).First().Id);
            THEN_test_client_is_busy(ClientsCollection.Clients.First(client => client.Id == registeredClient.Id));
        }
        
        // ======================================== Lack of pending test runs ========================================================
        [MbUnit.Framework.Test][NUnit.Framework.Test][Fact]
        public void Should_provide_a_task_to_test_client_if_the_client_matches_the_rule_and_there_are_no_test_runs()
        {
            TestRunQueue.TestRuns.Skip(1).First().Status = TestRunStatuses.Running;
            var expectedTask = GIVEN_Loaded_TestTask(5, "task name", false, TestTaskStatuses.New, true, _testClientHostnameExpected, 0);
            var testClient = GIVEN_Registered_TestClient_as_json(_testClientHostnameExpected, _testClientUsernameExpected);
            
            var actualTask = WHEN_Getting_task_as_json(testClient.Id);
            
            THEN_HttpResponse_Is_Ok();
            THEN_TestTask_Properties_Equal_To(expectedTask, actualTask, TestTaskStatuses.Running);
            THEN_test_client_is_busy(ClientsCollection.Clients.First(client => client.Id == testClient.Id));
        }
        // ============================================================================================================================
        ITestClient GIVEN_Registered_TestClient_as_json(string hostname, string username)
        {
            var testClient = new TestClient { Hostname = hostname, Username = username };
            response = _browser.Post(UrlList.TestClientRegistrationPoint_absPath, with => {
                                            with.JsonBody<ITestClient>(testClient);
                                            with.Accept("application/json");
                                        });
            testClient = response.Body.DeserializeJson<TestClient>();
            
            var clientSettings = ClientSettings.Instance;
            clientSettings.ServerUrl = @"http://localhost:12340";
            clientSettings.StopImmediately = false;
            
            clientSettings.ClientId = testClient.Id;
            // clientSettings.StopImmediately = false;
            
            
            
            return testClient;
        }
        
        ITestTask GIVEN_Loaded_TestTask(int id, string taskName, bool finished, TestTaskStatuses status, bool isActive, string rule, int afterTask)
        {
            var task = new TestTask {
                Id = id,
                Name = taskName,
                TaskFinished = finished,
                IsActive = isActive,
                TaskStatus = status,
                Rule = rule,
                AfterTask = afterTask,
                WorkflowId = _workflow.Id,
                TestRunId = _testRun.Id
            };
            TaskPool.Tasks.Add(task);
            return task;
        }
        
        ITestTask GIVEN_Allocated_TestTask(int id, string taskName, bool finished, TestTaskStatuses status, bool isActive, string rule, int afterTask)
        {
            var task = new TestTask {
                Id = id,
                Name = taskName,
                TaskFinished = finished,
                IsActive = isActive,
                TaskStatus = status,
                Rule = rule,
                AfterTask = afterTask,
                WorkflowId = _workflow.Id,
                TestRunId = _testRun.Id
            };
            TaskPool.TasksForClients.Add(task);
            return task;
        }
        
        // TODO: duplicated
        void WHEN_SendingDeregistration_as_json(ITestClient testClient)
        {
            _browser.Delete(UrlList.TestClients_Root + "/" + testClient.Id, with => with.Accept("application/json"));
        }
        
        void WHEN_SendingDeregistration_as_xml(TestClient testClient)
        {
            _browser.Delete(UrlList.TestClients_Root + "/" + testClient.Id, with => with.Accept("application/xml"));
        }
        
        // 20141020 squeezing a task to its proxy
        TestTask WHEN_Getting_task_as_json(Guid clientId)
        // TestTaskProxy WHEN_Getting_task_as_json(int clientId)
        {
            response = _browser.Get(UrlList.TestTasks_Root + "/" + clientId, with => with.Accept("application/json"));
            // 20141020 squeezing a task to its proxy
            var actualTask = response.Body.DeserializeJson<TestTask>();
            // var actualTask = response.Body.DeserializeJson<TestTaskProxy>();
            // var actualTask = response.Body.DeserializeJson<TestTaskCodeProxy>();
            if (null == actualTask) return actualTask;
            actualTask.TaskStatus = TestTaskStatuses.Running;
            // emulates actualTask.StartTimer();
            _startTime = DateTime.Now;
            actualTask.StartTime = _startTime;
            _browser.Put(UrlList.TestTasks_Root + "/" + actualTask.Id, with => {
                                    // 20141020 squeezing a task to its proxy
                                       with.JsonBody<ITestTask>(actualTask);
                                       // with.JsonBody<ITestTaskProxy>(actualTask);
                                       // with.JsonBody<ITestTaskStatusProxy>((actualTask as ITestTask).SqueezeTaskToTaskStatusProxy());
                                       with.Accept("application/json");
                                   });
            return actualTask;
        }
        
        void WHEN_Removing_Registered_Client_On_Server(ITestClient registeredClient)
        {
            ClientsCollection.Clients.RemoveAll(client => client.Id == registeredClient.Id);
        }
        
        // 20141020 squeezing a task to its proxy
        void WHEN_Finishing_Task_as_json(TestTask actualTask)
        // void WHEN_Finishing_Task_as_json(TestTaskProxy actualTask)
        {
            actualTask.TaskStatus = TestTaskStatuses.CompletedSuccessfully;
            actualTask.TaskFinished = true;
            _browser.Put(UrlList.TestTasks_Root + "/" + actualTask.Id, with => {
                with.Accept("application/json");
                // 20141020 squeezing a task to its proxy
                with.JsonBody<ITestTask>(actualTask);
                // with.JsonBody<ITestTaskProxy>(actualTask);
            });
        }
        
        // 20141020 squeezing a task to its proxy
        void WHEN_Failing_Task_as_json(TestTask actualTask)
        // void WHEN_Failing_Task(TestTaskProxy actualTask)
        {
            actualTask.TaskStatus = TestTaskStatuses.Interrupted;
            actualTask.TaskFinished = true;
            // 20141020 squeezing a task to its proxy
            _browser.Put(UrlList.TestTasks_Root + "/" + actualTask.Id, with => {
                             with.Accept("application/json");
                             with.JsonBody<ITestTask>(actualTask);
                         });
            // _browser.Put(UrnList.TestTasks_Root + "/" + actualTask.Id, with => with.JsonBody<ITestTaskProxy>(actualTask));
        }
        
        void THEN_HttpResponse_Is_Ok()
        {
            Xunit.Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        
        void THEN_HttpResponse_Is_NotFound()
        {
            Xunit.Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        void THEN_HttpResponse_Is_ExpectationFailed()
        {
            Xunit.Assert.Equal(HttpStatusCode.ExpectationFailed, response.StatusCode);
        }
        
        // 20141020 squeezing a task to its proxy
        void THEN_TestTask_Properties_Equal_To(ITestTask expectedTask, ITestTask actualTask)
        // void THEN_TestTask_Properties_Equal_To(ITestTask expectedTask, ITestTaskProxy actualTask)
        {
            Xunit.Assert.Equal(expectedTask.Id, actualTask.Id);
            Xunit.Assert.Equal(expectedTask.Name, actualTask.Name);
            Xunit.Assert.Equal(expectedTask.TaskStatus, actualTask.TaskStatus);
            // 20150112
            // Xunit.Assert.Equal(expectedTask.TaskFinished, actualTask.TaskFinished);
            // 20141020 squeezing a task to its proxy
            Xunit.Assert.Equal(expectedTask.IsActive, actualTask.IsActive);
            // Xunit.Assert.Equal(_startTime, actualTask.StartTime);
        }
        
        // 20141020 squeezing a task to its proxy
        void THEN_TestTask_Properties_Equal_To(ITestTask expectedTask, ITestTask actualTask, TestTaskStatuses status)
        // void THEN_TestTask_Properties_Equal_To(ITestTask expectedTask, ITestTaskProxy actualTask, TestTaskStatuses status)
        {
            expectedTask.TaskStatus = status;
            THEN_TestTask_Properties_Equal_To(expectedTask, actualTask);
        }
        
        // 20141020 squeezing a task to its proxy
        void THEN_TestTask_Is_Null(ITestTask task)
        // void THEN_TestTask_Is_Null(ITestTaskProxy task)
        {
            Xunit.Assert.Equal(null, task);
        }
        
        void THEN_test_client_is_busy(ITestClient testClient)
        {
            Xunit.Assert.Equal(TestClientStatuses.Running, testClient.Status);
        }
        
        void THEN_test_client_is_free(ITestClient testClient)
        {
            Xunit.Assert.Equal(TestClientStatuses.NoTasks, testClient.Status);
        }
        
        void THEN_testRun_isCompleted()
        {
            Xunit.Assert.Equal(true, _testRun.IsCompleted());
        }
    }
}
