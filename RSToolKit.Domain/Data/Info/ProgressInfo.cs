using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.Domain.Data
{
    /// <summary>
    /// Holds information the progress of an action taking place.
    /// </summary>
    public class ProgressInfo
        : IProgressInfo
    {
        /// <summary>
        /// The process key that the progress pertains to.
        /// </summary>
        public Guid Key
        {
            get { return this._key; }
            set
            {
                if (IsSynchronous)
                {
                    lock (this._lock)
                    {
                        this._key = value;
                    }
                }
                else
                {
                    this._key = value;
                }
            }
        }

        /// <summary>
        /// The fraction to move forward per tick.
        /// </summary>
        public float FractionPerTick { get; protected set; }

        /// <summary>
        /// The progress of the action taking place.
        /// </summary>
        public float Progress
        {
            get { return this._progress; }
            protected set
            {
                if (IsSynchronous)
                {
                    lock (this._lock)
                    {
                        this._progress = value;
                    }
                }
                else
                {
                    this._progress = value;
                }
            }
        }
        /// <summary>
        /// The status message of the progress.
        /// </summary>
        public string Message
        {
            get { return this._message; }
            protected set
            {
                if (IsSynchronous)
                {
                    lock (this._lock)
                    {
                        this._message = value;
                    }
                }
                else
                {
                    this._message = value;
                }
            }
        }
        /// <summary>
        /// The status details of the progress.
        /// </summary>
        public string Details
        {
            get { return this._details; }
            protected set
            {
                if (IsSynchronous)
                {
                    lock (this._lock)
                    {
                        this._details = value;
                    }
                }
                else
                {
                    this._details = value;
                }
            }
        }
        /// <summary>
        /// Flag for full completion.
        /// </summary>
        public bool Complete { get; set; }
        /// <summary>
        /// Determines if the item is synchronous or not.
        /// </summary>
        public bool IsSynchronous
        {
            get
            {
                return this._lock != null;
            }
        }

        /// <summary>
        /// The lock if it is synchronous.
        /// </summary>
        protected object _lock;
        /// <summary>
        /// The message of the item.
        /// </summary>
        protected string _message;
        /// <summary>
        /// The details of the item.
        /// </summary>
        protected string _details;
        /// <summary>
        /// The progress of the item.
        /// </summary>
        protected float _progress;
        /// <summary>
        /// The key of the process.
        /// </summary>
        protected Guid _key;

        /// <summary>
        /// Creates a new progress info class.
        /// </summary>
        /// <param name="synchronous">[optional] If this needs to be thread safe.</param>
        /// <param name="key">[optional] The key fo the process being referenced.</param>
        public ProgressInfo(bool synchronous = true, Guid? key = null)
        {
            this._progress = 0F;
            this._message = "Working";
            this._lock = synchronous ? new object() : null;
            this._key = Guid.Empty;
            this._details = "";
            Complete = false;
        }

        /// <summary>
        /// Resets the progress back to zero.
        /// </summary>
        /// <param name="message">[optional] The message to set.</param>
        /// <param name="details">[optional] The details to set.</param>
        public void Reset(string message = null, string details = null)
        {
            Progress = 0;
            Message = message ?? Message;
            Details = details ?? Details;
        }

        /// <summary>
        /// Updates the progress with the specified values.
        /// </summary>
        /// <param name="progress">[optional] The fraction of completion.</param>
        /// <param name="message">[optional] The message.</param>
        public void Update(float progress = 0F, string message = null, string details = null)
        {
            Progress = progress;
            Message = message ?? Message;
            Details = details ?? Details;
        }

        /// <summary>
        /// Updates the progress by adding the passed progress to the current and setting the message.
        /// </summary>
        /// <param name="progress">[optional] The fraction of completion.</param>
        /// <param name="message">[optional] The message.</param>
        public void UpdateAdd(float progress = 0F, string message = null, string details = null)
        {
            Progress += progress;
            Message = message ?? Message;
            Details = details ?? Details;
        }

        /// <summary>
        /// Sets the tick fraction by the amount of ticks to completion.
        /// </summary>
        /// <param name="totalTicks">The total number of ticks.</param>
        /// <returns>The tick fraction that was calculated.</returns>
        public float SetTickFraction(long totalTicks)
        {
            Progress = 0;
            FractionPerTick = 1F / (float)totalTicks;
            return FractionPerTick;
        }
        /// <summary>
        /// Moves the progressional fraction forward one tick.
        /// </summary>
        /// <param name="message">[optional] The message of the tick.</param>
        /// <param name="details">[optional] The details of the tick.</param>
        /// <returns>The new progressional fraction.</returns>
        public float Tick(string message = null, string details = null)
        {
            Progress += FractionPerTick;
            if (Progress > 1)
                Progress = 1;
            Message = message ?? Message;
            Details = details ?? Details;
            return Progress;
        }

        /// <summary>
        /// Updates the message and details.
        /// </summary>
        /// <param name="message">[optional] The message of the tick.</param>
        /// <param name="details">[optional] The details of the tick.</param>
        /// <returns>The new progressional fraction.</returns>
        public void UpdateMessage(string message = null, string details = null)
        {
            Message = message ?? Message;
            Details = details ?? Details;
        }

        /// <summary>
        /// Sets the progress as failed.
        /// </summary>
        /// <param name="message">The fail message.</param>
        public void Fail(string message = null)
        {
            Progress = -1F;
            Message = "Failed";
            Details = message ?? "The operation failed unexpectantly.";
        }

    }

    /// <summary>
    /// Holds information about the progress.
    /// </summary>
    public class ProgressMessage
    {
        /// <summary>
        /// The message of the progress.
        /// </summary>
        public string Message { get; protected set; }
        /// <summary>
        /// The details of the message.
        /// </summary>
        public string Details { get; protected set; }
        /// <summary>
        /// The progress.
        /// </summary>
        public float Progress { get; protected set; }
        /// <summary>
        /// Flag for the ultimate completion.
        /// </summary>
        public bool Complete { get; protected set; }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="progress">The progress.</param>
        /// <param name="details">The details of the request.</param>
        /// <param name="complete">[optional] If the process if fully complete.</param>
        public ProgressMessage(string message, float progress, string details, bool complete = false)
        {
            Message = message;
            Progress = progress;
            Details = details ?? "";
            Complete = complete;
        }
    }
}