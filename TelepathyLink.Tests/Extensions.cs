﻿using System;
using System.Threading.Tasks;

namespace TelepathyLink.Tests
{
    public static class Extensions
    {
        public static async Task TimeoutAfter(this Task task, int millisecondsTimeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(millisecondsTimeout)))
            {
                await task;
            }
            else
            {
                throw new TimeoutException();
            }
        }
    }
}