using Microsoft.EntityFrameworkCore;
using SAN_API.Data;
using SAN_API.Models;
using SAN_API.Services.KoboToolBox.ApiKoboToolBox;
using System.Globalization;
using System.Threading.Tasks;

namespace SAN_API.Helper
{
    public class InsertAnswerHelper
    {
        private readonly DataContext dataContext;

        public InsertAnswerHelper(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }
        public  async Task InsertAnswer(string uid)
        {
            var cleaned = await ApiKoboToolBox.AllAnswer(uid);

            if(cleaned.Count > 0)
            {
                foreach(var grp_rps in cleaned)
                {
                    KoboSoumissionModel soumission = new();
                    soumission.Uid = uid;
                    DateTime date_soumission = DateTime.ParseExact(grp_rps.Reponse[1], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    soumission.Date_soumission = date_soumission;
                    await dataContext.Soumissions.AddAsync(soumission);
                    if(grp_rps.Reponse.Count > 2)
                    {
                        var grp_racine = await dataContext.GroupeKobo.FirstOrDefaultAsync(g => g.Uid!.Equals(uid) && g.Nom_groupe!.Equals("Racine"));
                        if(grp_racine != null)
                        {
                            var questions = await dataContext.QuestionKobo.Where(q => q.Id_groupe!.Equals(grp_racine.Id.ToString().ToUpper())).ToListAsync();
                            int num_rps = 2;
                            foreach(var q in questions)
                            {
                                ReponseKoboModel reponse = new();
                                reponse.Id_soumission = soumission.Id.ToString().ToUpper();
                                reponse.Id_question = q.Id.ToString().ToUpper();
                                reponse.Reponse = grp_rps.Reponse[num_rps];
                                num_rps++;
                            }
                        }
                    }
                    if(grp_rps.GroupeResponse.Count > 0)
                    {
                        foreach(var one_grp in grp_rps.GroupeResponse)
                        {
                            var grp_bd = await dataContext.GroupeKobo.FirstOrDefaultAsync(g => g.Nom_technique!.Equals(one_grp.Nom_technique));
                            if(grp_bd != null)
                            {
                                var ques = await dataContext.QuestionKobo.Where(q => q.Id_groupe!.Equals(grp_bd.Id.ToString().ToUpper())).ToListAsync();
                                if(ques != null && ques.Count > 0 )
                                {
                                    foreach(var one_soumis in one_grp.Reponse)
                                    {
                                        if(one_soumis.ReponseQuestion != null && one_soumis.ReponseQuestion.Count > 0 && ques.Count == one_soumis.ReponseQuestion.Count)
                                        {
                                            int position_question = 0;
                                            foreach (var o_q in one_soumis.ReponseQuestion)
                                            {
                                                ReponseKoboModel reponse_one = new();
                                                reponse_one.Id_soumission = soumission.Id.ToString().ToUpper();
                                                reponse_one.Id_question = ques[position_question].Id.ToString().ToUpper();
                                                reponse_one.Reponse = o_q;
                                                await dataContext.ReponseKobo.AddAsync(reponse_one);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }


                }

                await dataContext.SaveChangesAsync();
            }
        }
    }
}
