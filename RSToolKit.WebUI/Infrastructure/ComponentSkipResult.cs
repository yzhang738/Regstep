using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RSToolKit.WebUI.Infrastructure
{
    public class ComponentSkipResult
    {
        protected bool skip;
        protected bool nullOut;

        public bool Skip
        {
            get
            {
                return skip;
            }
            set
            {
                skip = value;
            }
        }
        public bool NullOut
        {
            get
            {
                return nullOut;
            }
            set
            {
                if (value)
                    skip = true;
                nullOut = value;
            }
        }
        public bool Hide { get; set; }

        public ComponentSkipResult()
        {
            skip = false;
            nullOut = false;
            Hide = false;
        }
    }
}