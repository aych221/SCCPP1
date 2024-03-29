﻿using SCCPP1.Database.Requests;
using System.Security.Cryptography;

namespace SCCPP1.User.Data
{
    public class CertificationData : OwnerRecordData
    {

        private string _institution;
        public string Institution
        {
            get { return _institution; }
            set
            { SetField(ref _institution, value); }
        }

        public int InstitutionID { get; set; }


        private string _certificationType;
        public string CertificationType
        {
            get { return _certificationType; }
            set { SetField(ref _certificationType, value); }
        }

        public int CertificationTypeID { get; set; }



        private string? _description;
        public string? Description
        {
            get { return _description; }
            set { SetField(ref _description, value); }
        }


        private Location _location;
        public Location Location
        {
            get { return _location; }
            set { SetField(ref _location, value); }
        }

        private DateOnly _startDate;
        public DateOnly StartDate
        {
            get { return _startDate; }
            set { SetField(ref _startDate, value); }
        }

        private DateOnly _endDate;
        public DateOnly EndDate
        {
            get { return _endDate; }
            set { SetField(ref _endDate, value); }
        }


        public CertificationData(Account owner, int recordID = -1) : base(owner, recordID)
        {
            InstitutionID = -1;
            CertificationTypeID = -1;
        }


        public CertificationData(Account owner, int recordID, string institution, int institutionID, string certificationType, int certificationTypeID, string description, Location location, DateOnly startDate, DateOnly endDate) : this(owner, recordID)
        {
            this.Institution = institution;
            this.InstitutionID = institutionID;
            this.CertificationType = certificationType;
            this.CertificationTypeID = certificationTypeID;
            this.Description = description;
            this.Location = location;
            this.StartDate = startDate;
            this.EndDate = endDate;

            this.IsUpdated = true;
        }

        //for creating from user input
        public CertificationData(Account owner, string institution, string certificationType, string? description, Location location, DateOnly startDate, DateOnly endDate) :
            this(owner, -1, institution, -1, certificationType, -1, description, location, startDate, endDate)
        {
            this.NeedsSave = true;
        }

        //for loading from db
        public CertificationData(Account owner, int recordID, int institutionID, int certificationTypeID, string? description, Location location, DateOnly startDate, DateOnly endDate) :
            this(owner, recordID, null, institutionID, null, certificationTypeID, description, location, startDate, endDate)
        {
            this.NeedsSave = false;
            //maybe use IsUpdated = false flag to let DB know we need null values filled?
        }


        public override bool Save()
        {
            //save resources if we don't need to save.
            /* if (!NeedsSave)
                 return true;*/


            if (Program.DbRequestSystem)
                return !(NeedsSave = !(IsUpdated = DbRequestManager.Save(this)));
            else
            {
                return !(NeedsSave = !(IsUpdated = DatabaseConnector.SaveCertification(this)));
            }
        }


        /// <summary>
        /// Deletes the profile record.
        /// </summary>
        /// <returns>true if record was removed from database, false otherwise.</returns>
        public override bool Delete()
        {
            return Remove = NeedsSave = IsUpdated = true;
        }

    }
}
