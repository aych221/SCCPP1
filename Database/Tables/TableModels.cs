using SCCPP1.Database.Entity;

namespace SCCPP1.Database.Tables
{
    public class TableModels
    {

        /// <summary>
        /// The colleagues table model
        /// </summary>
        public readonly DbTable Colleagues;


        /// <summary>
        /// The municipalities table model
        /// </summary>
        public readonly DbTable Municipalities;


        /// <summary>
        /// The states table model
        /// </summary>
        public readonly DbTable States;


        /// <summary>
        /// The education_types table model
        /// </summary>
        public readonly DbTable EducationTypes;


        /// <summary>
        /// The institutions table model
        /// </summary>
        public readonly DbTable Institutions;


        /// <summary>
        /// The education_histories table model
        /// </summary>
        public readonly DbTable EducationHistories;


        /// <summary>
        /// The employers table model
        /// </summary>
        public readonly DbTable Employers;
        

        /// <summary>
        /// The job_titles table model
        /// </summary>
        public readonly DbTable JobTitles;


        /// <summary>
        /// The work_histories table model
        /// </summary>
        public readonly DbTable WorkHistories;


        /// <summary>
        /// The skills table model
        /// </summary>
        public readonly DbTable Skills;


        /// <summary>
        /// The skill_categories table model
        /// </summary>
        public readonly DbTable SkillCategories;


        /// <summary>
        /// The colleague_skills table model
        /// </summary>
        public readonly DbTable ColleagueSkills;


        /// <summary>
        /// The profiles table model
        /// </summary>
        public readonly DbTable Profiles;


        public readonly List<DbTable> Tables;



        public TableModels()
        {
            Tables = new List<DbTable>();

            DbColumn profile_id, colleague_id, education_histories_id, education_type_id,
                work_histories_id, employer_id, job_title_id, colleague_skills_id,
                skill_id, skill_category_id, institution_id, municipality_id, state_id;


            //create common fields
            Field valF = new Field("val", typeof(string), true, true),
                startDateF = new Field("start_date", typeof(DateTime), false, false),
                endDateF = new Field("end_date", typeof(DateTime), false, false),
                descriptionF = new Field("description", typeof(string), false, false),
                municipalityF, stateF;

            //Create the colleagues table model
            Colleagues = new DbTable("colleagues",
                new Field("user_hash", typeof(string), true, true),
                new Field("role", typeof(int), true, false),
                new Field("name", typeof(string), true, false),
                new Field("email", typeof(string), false, false),
                new Field("phone", typeof(long), false, false),
                new Field("address", typeof(string), false, false),
                new Field("intro_narrative", typeof(string), false, false)
                );

            colleague_id = Colleagues.PrimaryKey;
            Tables.Add(Colleagues);


            //Create the municipalites table model
            Tables.Add(Municipalities = new DbTable("municipalities", valF));
            municipality_id = Municipalities.PrimaryKey;


            //Create the states table model
            States = new DbTable("states",
                new Field("name", typeof(string), true, true),
                new Field("abbreviation", typeof(string), true, true)
                );
            state_id = States.PrimaryKey;
            Tables.Add(States);


            //set fields to reuse
            municipalityF = new Field("municipality_id", typeof(int), false, false, municipality_id);
            stateF = new Field("state_id", typeof(int), false, false, state_id);



            //Create the institutions table model
            Tables.Add(Institutions = new DbTable("institutions", valF));
            institution_id = Institutions.PrimaryKey;


            //Create the institutions table model
            Tables.Add(EducationTypes = new DbTable("education_types", valF));
            education_type_id = EducationTypes.PrimaryKey;


            //Create the education_histories table model
            EducationHistories = new DbTable("education_histories",
                new Field("colleague_id", typeof(int), true, false, colleague_id),
                new Field("education_type_id", typeof(int), true, false, education_type_id),
                new Field("institution_id", typeof(int), true, false, institution_id),
                municipalityF,
                stateF,
                startDateF,
                endDateF,
                descriptionF
                );
            education_histories_id = EducationHistories.PrimaryKey;
            Tables.Add(EducationHistories);


            //Create the employers table model
            Tables.Add(Employers = new DbTable("employers", valF));
            employer_id = Employers.PrimaryKey;


            //Create the job_titles table model
            Tables.Add(JobTitles = new DbTable("job_titles", valF));
            job_title_id = JobTitles.PrimaryKey;


            //Create the education_histories table model
            WorkHistories = new DbTable("work_histories",
                new Field("colleague_id", typeof(int), true, false, colleague_id),
                new Field("employer_id", typeof(int), true, false, employer_id),
                new Field("job_title_id", typeof(int), true, false, job_title_id),
                municipalityF,
                stateF,
                startDateF,
                endDateF,
                descriptionF
                );
            work_histories_id = WorkHistories.PrimaryKey;
            Tables.Add(WorkHistories);


            //Create the skills table model
            Tables.Add(Skills = new DbTable("skills", valF));
            skill_id = Skills.PrimaryKey;


            //Create the skills table model
            Tables.Add(SkillCategories = new DbTable("skill_categories", valF));
            skill_category_id = SkillCategories.PrimaryKey;


            //Create the education_histories table model
            ColleagueSkills = new DbTable("colleague_skills",
                new Field("colleague_id", typeof(int), true, false, colleague_id),
                new Field("skill_id", typeof(int), true, false, skill_id),
                new Field("skill_category_id", typeof(int), false, false, skill_category_id),
                new Field("rating", typeof(int), false, false)
                );
            colleague_skills_id = ColleagueSkills.PrimaryKey;
            Tables.Add(ColleagueSkills);


            //Create the education_histories table model
            Profiles = new DbTable("profiles",
                new Field("colleague_id", typeof(int), true, false, colleague_id),
                new Field("title", typeof(string), true, false),
                new Field("education_histories_ids", typeof(string), false, false),
                new Field("work_histories_ids", typeof(int), false, false),
                new Field("colleague_skills_ids", typeof(string), false, false),
                new Field("ordering", typeof(string), false, false)
                );
            profile_id = ColleagueSkills.PrimaryKey;
            Tables.Add(Profiles);

            //TODO add after query is done
            //Colleagues.AddCols(new Field("main_profile_id", typeof(int), false, false, profile_id));

            foreach (DbTable t in Tables)
                Console.WriteLine(QueryGenerator.CreateTableSql(t));


        }

    }
}
