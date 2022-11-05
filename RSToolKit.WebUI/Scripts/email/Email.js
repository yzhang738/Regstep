﻿/*************************/
/* Edit Email Javascript */
/*************************/


var Email = function () {
    this.SortingId = -1;
    this.UId = '';
    this.Name = '';
    this.DateCreated = '';
    this.DateModified = '';
    this.ModificationToken = '';
    this.ModifiedBy = '';
    this.CompanyKey = '';
    this.EmailTemplateKey = '';
    this.FormKey = '';
    this.EmailCampaignKey = null;
    this.SavedListKey = null;
    this.ContactReportKey = null;
    this.EmailListKey = null;
    this.EmailAreas = [];
    this.Variables = [];
    this.Subject = '';
    this.Description = '';
    this.From = '';
    this.CC = '';
    this.BCC = '';
    this.EmailType = 0;
    this.SendTime = '';
    this.MaxSends = -1;
    this.RepeatSending = false;
    this.To = '';
    this.IntervalSeconds = '';
    this.PlainText = '';
}