using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FDM90.Singleton
{
    public class TaskListSingleton
    {
        private static TaskListSingleton _instance;

        public static TaskListSingleton Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TaskListSingleton();

                return _instance;
            }
        }

        public List<System.Threading.Tasks.Task> CurrentTasks = new List<System.Threading.Tasks.Task>();

        private TaskListSingleton() { }

    }
}