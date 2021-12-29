using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureFlags.APIs.ViewModels.Analytic
{
    public class TrackUserBehaviorEventParam
    {
        public string UserKey { get; set; }
        public long? UtcTimeStampFromClientEnd { get; set; }
        public TrackUserBehaviorClickEvent ClickEvent { get; set; }
        public TrackUserBehaviorPageViewEvent PageViewEvent { get; set; }
        public TrackCustomEvent CustomEvent { get; set; }
        public TrackPageStayDurationEvent PageStayDurationEvent { get; set; }
    }
    public enum MediaTypeEnum
    {
        WebAPP
    }
    public class TrackUserBehaviorEvent
    {
        public string MediaType { get; set; }
        public string EventType { get; set; }
        public string UserKey { get; set; }
        public long TimeStamp { get; set; }
        public long TimeStampFromClientEnd { get; set; }
        public TrackUserBehaviorClickEvent ClickEvent { get; set; }
        public TrackUserBehaviorPageViewEvent PageViewEvent { get; set; }
        public TrackCustomEvent CustomEvent { get; set; }
        public TrackPageStayDurationEvent PageStayDurationEvent { get; set; }
        public int EnvironmentId { get; set; }
    }

    public enum UserBehaviorEnum
    {
        Click,
        PageView,
        CustomEvent,
        PageStop
    }

    public class TrackUserBehaviorClickEvent
    {
        public string InnerText { get; set; }
        public string CssSelector { get; set; }
        public string ElementType { get; set; }
        public string Href { get; set; }
        public string Origin { get; set; }
        public string Hash { get; set; }
        public string Pathname { get; set; }
    }

    public class TrackUserBehaviorPageViewEvent
    {
        public string Href { get; set; }
	    public string Origin { get; set; }
	    public string Hash { get; set; }
	    public string Pathname { get; set; }
    }

    public class TrackCustomEvent
    {
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public string EventValue { get; set; }
    }

    public class TrackPageStayDurationEvent
    {
        public string Href { get; set; }
	    public string Origin { get; set; }
	    public string Hash { get; set; }
	    public string Pathname { get; set; }
	    public long Duration { get; set; }
    }
}

