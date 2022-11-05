using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSToolKit.Domain.Data.Info
{
    public class Progress
    {
        protected bool _hardFail;

        /// <summary>
        /// The process key that the progress pertains to.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// The fraction to move forward per tick.
        /// </summary>
        public float FractionPerTick { get; protected set; }
        /// <summary>
        /// The progress of the action taking place.
        /// </summary>
        public float Status { get; set; }
        /// <summary>
        /// The status message of the progress.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// The status details of the progress.
        /// </summary>
        public string Details { get; set; }
        /// <summary>
        /// Flag for full completion.
        /// </summary>
        public bool Complete { get; set; }
        /// <summary>
        /// The exception.
        /// </summary>
        public Exception Exception { get; set; }
        /// <summary>
        /// A flag to indicate that the item could not be loaded and should not be tried again without more inspection.
        /// </summary>
        public bool HardFail
        {
            get { return this._hardFail; }
            set
            {
                if (value)
                {
                    this._hardFail = Failed = true;
                }
                else
                {
                    this._hardFail = false;
                }
            }
        }

        /// <summary>
        /// A flag if the item failed to load.
        /// </summary>
        public bool Failed { get; set; }

        /// <summary>
        /// Creates a new progress info class.
        /// </summary>
        /// <param name="key">The key fo the process being referenced.</param>
        public Progress(string key)
            : this()
        {
            Id = key;
        }

        /// <summary>
        /// Creates a new progress info class.
        /// </summary>
        public Progress()
        {
            Status = 0F;
            Message = "Working";
            Complete = false;
            HardFail = false;
            Failed = false;
        }

        /// <summary>
        /// Resets the progress back to zero.
        /// </summary>
        /// <param name="message">[optional] The message to set.</param>
        /// <param name="details">[optional] The details to set.</param>
        public void Reset(string message = null, string details = null)
        {
            Status = 0;
            Message = message ?? Message;
            Details = details ?? Details;
            Complete = false;
        }

        /// <summary>
        /// Updates the progress with the specified values.
        /// </summary>
        /// <param name="progress">[optional] The fraction of completion.</param>
        /// <param name="message">[optional] The message.</param>
        public void Update(float progress = 0F, string message = null, string details = null)
        {
            Status = progress;
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
            Status += progress;
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
            Status = 0;
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
            Status += FractionPerTick;
            if (Status > 1)
                Status = 1;
            Message = message ?? Message;
            Details = details ?? Details;
            return Status;
        }

        public float JumpTicks(int jump, string message = null, string details = null)
        {
            Status += (FractionPerTick * jump);
            if (Status > 1)
                Status = 1;
            Message = message ?? Message;
            Details = details ?? Details;
            return Status;
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
            Status = -1F;
            Failed = true;
            Message = "Failed";
            Details = message ?? "The operation failed unexpectantly.";
        }

        /// <summary>
        /// Set the progress as HardFailed.
        /// </summary>
        /// <param name="e"></param>
        public void Fail(Exception e)
        {
            HardFail = true;
            Message = "Unrecoverable Failure";
            Details = e.Message;
        }


        public override bool Equals(object obj)
        {
            if (obj is Progress)
                return ((Progress)obj).Id == Id;
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
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
