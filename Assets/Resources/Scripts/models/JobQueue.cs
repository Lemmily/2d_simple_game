using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class JobQueue {
    Queue<Job> jobQueue;

    Action<Job> cbJobCreated;
    
    public JobQueue () {
        jobQueue = new Queue<Job>();
    }

    public void Enqueue(Job j) {
        if (j.JobTime < 0) {
            //if job has non positive time. Just try insta complete instead.
            //non queueable, just do!
            j.DoWork(0);
            return;
        }
        jobQueue.Enqueue(j);

        if (cbJobCreated != null)
            cbJobCreated(j);
    }

    public Job Dequeue() {
        if (jobQueue.Count == 0)
            return null;
        return jobQueue.Dequeue();
    }

    public void Remove(Job j)
    {
        if (jobQueue.Contains(j) == false) {
            Debug.LogError("JobQueue:-  Tried to remove a job that doesn't exist!");
            return;
        }
        List<Job> jobs = new List<Job>(jobQueue);
        jobs.Remove(j);
        jobQueue = new Queue<Job>(jobs);
    }

    internal void RegisterJobCreationCallback(Action<Job> cb) {
        cbJobCreated += cb;
    }
    internal void UnregisterJobCreationCallback(Action<Job> cb) {
        cbJobCreated -= cb;
    }       
}
