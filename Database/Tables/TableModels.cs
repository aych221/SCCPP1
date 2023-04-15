using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using SCCPP1.Database.Entity;

namespace SCCPP1.Database.Tables
{
    public class TableModels
    {

        public readonly Dictionary<string, DbTable> Tables;



        public TableModels()
        {
            Tables = new();

            DbColumn profile_id, colleague_id, municipality_id, state_id,
                skill_id, skill_category_id, colleague_skills_id,
                institution_id, education_type_id, education_histories_id,
                employer_id, job_title_id, work_histories_id,
                certification_type_id, colleague_certifications_id;


            //create common fields
            Field valF = new Field("val", typeof(string), true, true),
                startDateF = new Field("start_date", typeof(DateTime), false, false),
                endDateF = new Field("end_date", typeof(DateTime), false, false),
                descriptionF = new Field("description", typeof(string), false, false),
                municipalityF, stateF;

            //Create the colleagues table model
            colleague_id = CreateTable(Tables, "colleagues",
                new Field("user_hash", typeof(string), true, true),
                new Field("role", typeof(int), true, false),
                new Field("name", typeof(string), true, false),
                new Field("email", typeof(string), false, false),
                new Field("phone", typeof(long), false, false),
                new Field("address", typeof(string), false, false),
                new Field("intro_narrative", typeof(string), false, false)
                ).PrimaryKey;


            //Create the municipalites table model
            municipality_id = CreateTable(Tables, "municipalities", valF).PrimaryKey;


            //Create the states table model
            state_id = CreateTable(Tables, "states",
                new Field("name", typeof(string), true, true),
                new Field("abbreviation", typeof(string), true, true)
                ).PrimaryKey;


            //set fields to reuse
            municipalityF = new Field("municipality_id", typeof(int), false, false, municipality_id);
            stateF = new Field("state_id", typeof(int), false, false, state_id);


            //Create the skills table model
            skill_id = CreateTable(Tables, "skills", valF).PrimaryKey;


            //Create the skills table model
            skill_category_id = CreateTable(Tables, "skill_categories", valF).PrimaryKey;


            //Create the education_histories table model
            colleague_skills_id = CreateTable(Tables, "colleague_skills",
                new Field("colleague_id", typeof(int), true, false, colleague_id),
                new Field("skill_id", typeof(int), true, false, skill_id),
                new Field("skill_category_id", typeof(int), false, false, skill_category_id),
                new Field("rating", typeof(int), false, false)
                ).PrimaryKey;



            //Create the institutions table model
            institution_id = CreateTable(Tables, "institutions", valF).PrimaryKey;


            //Create the education_types table model
            education_type_id = CreateTable(Tables, "education_types", valF).PrimaryKey;


            //Create the education_histories table model
            education_histories_id = CreateTable(Tables, "education_histories",
                new Field("colleague_id", typeof(int), true, false, colleague_id),
                new Field("education_type_id", typeof(int), true, false, education_type_id),
                new Field("institution_id", typeof(int), true, false, institution_id),
                municipalityF,
                stateF,
                startDateF,
                endDateF,
                descriptionF
                ).PrimaryKey;



            //Create the certification_types table model
            certification_type_id = CreateTable(Tables, "certification_types", valF).PrimaryKey;


            //Create the colleague_certifications table model
            colleague_certifications_id = CreateTable(Tables, "colleague_certifications",
                new Field("colleague_id", typeof(int), true, false, colleague_id),
                new Field("education_type_id", typeof(int), true, false, certification_type_id),
                new Field("institution_id", typeof(int), true, false, institution_id),
                municipalityF,
                stateF,
                startDateF,
                endDateF,
                descriptionF
                ).PrimaryKey;


            //Create the employers table model
            employer_id = CreateTable(Tables, "employers", valF).PrimaryKey;


            //Create the job_titles table model
            job_title_id = CreateTable(Tables, "job_titles", valF).PrimaryKey;


            //Create the work_histories table model
            work_histories_id = CreateTable(Tables, "work_histories",
                new Field("colleague_id", typeof(int), true, false, colleague_id),
                new Field("employer_id", typeof(int), true, false, employer_id),
                new Field("job_title_id", typeof(int), true, false, job_title_id),
                municipalityF,
                stateF,
                startDateF,
                endDateF,
                descriptionF
                ).PrimaryKey;


            //Create the profiles table model
            profile_id = CreateTable(Tables, "profiles",
                new Field("colleague_id", typeof(int), true, false, colleague_id),
                new Field("title", typeof(string), true, false),
                new Field("colleague_skills_ids", typeof(string), false, false),
                new Field("education_histories_ids", typeof(string), false, false),
                new Field("colleague_certifications_ids", typeof(string), false, false),
                new Field("work_histories_ids", typeof(int), false, false),
                new Field("ordering", typeof(string), false, false)
                ).PrimaryKey;




            //TODO add after query is done
            //may be able to just turn pragma off then do this.
            Tables["colleagues"].AddCols(new Field("main_profile_id", typeof(int), false, false, profile_id));

            foreach (DbTable t in Tables.Values)
                Console.WriteLine(QueryGenerator.CreateTableSql(t));

        }


        protected DbTable CreateTable(Dictionary<string, DbTable> dict, string name, params Field[] fields)
        {
            DbTable t = new DbTable(name, NextAlias(), fields);
            dict.TryAdd(name, t);
            return t;
        }


        protected void LoadTables()
        {

        }



        /// <summary>
        /// Alias indexers. Used to keep track of the current alias step.
        /// </summary>
        private int _ai, _aj;

        /// <summary>
        /// Alias chars, does not contain vowels to ensure no keywords are generated.
        /// </summary>
        private char[] _aliasChars = new char[] {'b', 'c', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'm', 'n', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'z'};
        private string NextAlias()
        {
            if (_ai >= _aliasChars.Length)
                _ai = 0;
            if (_aj >= _aliasChars.Length)
            {
                _aj = 0;
                _ai++;
            }
            return $"{_aliasChars[_ai]}{_aliasChars[_aj++]}";
        }
    }
}
