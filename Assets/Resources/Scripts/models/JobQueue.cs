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
        j.timesQueued++;

        if (cbJobCreated != null)
            cbJobCreated(j);
    }


    //For use ONLY when returning the job to the queue.
    public void Requeue(Job j)
    {
        //if (j.timesQueued > 50)
        //{
        //    Debug.Log("JobQueue:- Cancelled job because " +
        //        "it had been re-queued more than 50 times");
        //    if (j.tile.hasFurniture())
        //    {
        //        Debug.Log("Cancelled Job had furniture");
        //        j.tile.furniture.RemoveJob(j);
        //    }
        //    else
        //    {
        //        Debug.Log("Cancelled Job didn't have furniture");
        //        j.CancelJob();
        //    }
        //}

        //dont know how to stop this being used instead of enqueue.
        jobQueue.Enqueue(j);
        j.timesQueued++;
        
    }

    public Job Dequeue() {
        if (jobQueue.Count == 0)
            return null;
        return jobQueue.Dequeue();
    }

    public void Remove(Job j)
    {
        Debug.Log("JobQueue:-------- trying to remove job:- " + j);
        if (jobQueue.Contains(j) == false) {
            Debug.LogError("JobQueue:-  Tried to remove a job that doesn't exist!" + j);
            //could be cause this job is being worked by a character.
            return;
        }
        List<Job> jobs = new List<Job>(jobQueue);
        jobs.Remove(j);
        jobQueue = new Queue<Job>(jobs);
        Debug.Log("JobQueue:-------- removed job:- " + j);
    }


    public void ClearJobs()
    {
        jobQueue.Clear();
    }
    internal void RegisterJobCreationCallback(Action<Job> cb) {
        cbJobCreated += cb;
    }
    internal void UnregisterJobCreationCallback(Action<Job> cb) {
        cbJobCreated -= cb;
    }

    public bool HasJob(Job job)
    {
        return jobQueue.Contains(job);
    }
}
