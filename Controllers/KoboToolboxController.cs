using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SAN_API.Data;
using SAN_API.Helper;
using SAN_API.Models;
using SAN_API.Services.KoboToolBox.ApiKoboToolBox;
using SAN_API.Services.KoboToolBox.ModelKoboTollbox;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SAN_API.Controllers
{
    public class KoboToolboxController : Controller
    {
        public readonly DataContext dataContext;
        private static List<string> verification_question = null;
        public KoboToolboxController(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }
        [HttpGet("test/apikey")]
        public async Task<IActionResult> TestApi()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out Guid id))
            {
                return Problem(
                        statusCode: StatusCodes.Status401Unauthorized,
                        title: "User Id invalid",
                        detail: "Id de l'utilisateur non valide"
                    );
            }
            var apikey = await dataContext.ApiKeyKobo.FirstOrDefaultAsync(a => a.Id_utilisateur!.ToUpper().Equals(userId.ToString().ToUpper()));
            if(apikey == null)
            {
                return Ok(false);
            }
            else
            {
                ApiKoboToolBox.EnterApi(apikey.Key!);
                return Ok(true);
            }
        }
        [HttpDelete("kobo/delete/api")]
        public async Task<IActionResult> DeleteApi()
        {
            if (dataContext is null || dataContext.ApiKeyKobo is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur Serveur",
                        detail: "Le contexte de données est introuvable"
                    );
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out Guid id))
            {
                return Problem(
                        statusCode: StatusCodes.Status401Unauthorized,
                        title: "User Id invalid",
                        detail: "Id de l'utilisateur non valide"
                    );
            }
            var key = await dataContext.ApiKeyKobo.FirstOrDefaultAsync(k => k.Id_utilisateur!.ToUpper().Equals(userId.ToString().ToUpper()));
            if(key is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Key introuvable",
                        detail: "Votre key api est introuvable"
                    );
            }
            dataContext.ApiKeyKobo.Remove(key);
            await dataContext.SaveChangesAsync();
            ApiKoboToolBox.ApiKey = null;
            return Ok(true);
        }
        [HttpPost("kobo/add/api")]
        public async Task<IActionResult> AddApi([FromBody]ApiKeyKoboHelper model)
        {
            if (model is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Requete invalide",
                    detail: "La requête est invalide ou manquant"
                );
            }
            if (dataContext is null || dataContext.ApiKeyKobo is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur Serveur",
                        detail: "Le contexte de données est introuvable"
                    );
            }
            ApiKeyKoboModel new_api = new();
            new_api.Id_utilisateur = model.Id_utilisateur!.ToUpper();
            new_api.Key = model.Key;
            bool keyValid = true;
            try
            {
                ApiKoboToolBox.EnterApi(new_api.Key!);
                await ApiKoboToolBox.GetAllProject();
            }
            catch(HttpRequestException)
            {
                keyValid = false;
            }
            if (!keyValid)
            {
                return Problem(
                        statusCode: StatusCodes.Status401Unauthorized,
                        title: "Erreur api key",
                        detail: "votre clé api est incorrecte ou problème de connexion"
                    );
            }

            await dataContext.ApiKeyKobo.AddAsync(new_api);
            await dataContext.SaveChangesAsync();
            return Ok(true);

        }
        [HttpGet("kobo/projetcts")]
        public async Task<IActionResult> GetAllProjects()
        {
            if (dataContext is null ||
                dataContext.FormulairesKobo is null ||
                dataContext.GroupeKobo is null ||
                dataContext.QuestionKobo is null ||
                dataContext.ReponseKobo is null ||
                dataContext.Soumissions is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur Serveur",
                        detail: "Le contexte de données est introuvable"
                    );
            }

            //objet hors ligne
            NoConnexHelper no_connect = new();

            List<FormulaireKoboModel> liste_formulaires = new();
            liste_formulaires = await dataContext.FormulairesKobo.ToListAsync();
            if (liste_formulaires is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Formulaire non trouvée",
                        detail: "Aucun formulaire trouvée"
                    );
            }

            //liste des formulaire dans la base de données si on est hors ligne
            no_connect.Liste_formulaire = liste_formulaires;

            List<KoboAssetModel> liste_assets = new();
            try
            {
                //en ligne
                liste_assets = await ApiKoboToolBox.GetAllProject();
            }
            catch (HttpRequestException)
            {
                //hors ligne
                no_connect.Isconnect = false;
                return Ok(no_connect);
            }
            
            if (liste_assets.Count > 0 || liste_formulaires.Count > 0)
            {
                //ici, on est en ligne et on synchronise les données

                liste_assets.RemoveAt(liste_assets.Count - 1);
                RetoureKoboFormulaireHelper retoureKoboFormulaire = new();
                bool isLess = false;
                bool isMore = false;
                //si plus
                var set_form = new HashSet<string>(liste_formulaires.Select(f => f.Uid!));
                foreach (var lst_asst in liste_assets)
                {
                    if (!set_form.Contains(lst_asst.Uid!))
                    {
                        isMore = true;
                        retoureKoboFormulaire.koboAssets!.Add(lst_asst);
                    }
                }
                var set_assets = new HashSet<string>(liste_assets.Select(f => f.Uid!));
                foreach (var lst_form in liste_formulaires)
                {
                    if (!lst_form.Not_del)
                    {
                        if (!set_assets.Contains(lst_form.Uid!))
                        {
                            isLess = true;
                            retoureKoboFormulaire.koboForm!.Add(lst_form);
                            retoureKoboFormulaire.Alert = true;
                        }
                    }
                }
                if (isLess || isMore)
                {
                    retoureKoboFormulaire.IsLess = isLess;
                    retoureKoboFormulaire.IsMore = isMore;
                    retoureKoboFormulaire.CountNew = retoureKoboFormulaire.koboAssets!.Count;
                    retoureKoboFormulaire.CountDel = retoureKoboFormulaire.koboForm!.Count;
                    return Ok(retoureKoboFormulaire);
                }
            }
            no_connect.Isconnect = true;
            return Ok(no_connect);
        }

        //Nouvelle formulaire
        [HttpPost("kobo/new/synchronisation")]
        public async Task<IActionResult> NewSynchronisation([FromBody] RetoureKoboFormulaireHelper model)
        {
            if (model is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Requete invalide",
                        detail: "La requête est invalide ou manquant"
                    );
            }
            if (dataContext is null ||
                dataContext.FormulairesKobo is null ||
                dataContext.GroupeKobo is null ||
                dataContext.QuestionKobo is null ||
                dataContext.ReponseKobo is null ||
                dataContext.Soumissions is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur Serveur",
                        detail: "Le contexte de données est introuvable"
                    );
            }

            
            List<FormulaireKoboModel> formulaires = new();
            List<GroupeKoboModel> grps = new();
            List<QuestionKoboModel> qsts = new();
            List<ReponseKoboModel> rps = new();
            List<KoboSoumissionModel> soumis = new();
            
            /*
            if (model.IsLess)
            {
                foreach(var frm in model.koboForm!)
                {
                    if (model.NotDel!.Contains(frm.Uid!))
                    {
                        var formNotDel = await dataContext!.FormulairesKobo!.FirstOrDefaultAsync(f => f.Uid!.Equals(frm.Uid));
                        FormulaireKoboModel frms = new();
                        if(formNotDel is not null)
                        {
                            frms = formNotDel;
                            frms.Not_del = true;
                        }
                    }
                    else
                    {   
                        soumis = await dataContext.Soumissions.Where(s => s.Uid!.Equals(frm.Uid)).ToListAsync();
                        if(soumis is not null && soumis.Count != 0)
                        {
                            dataContext.Soumissions.RemoveRange(soumis);
                        }
                        grps = await dataContext.GroupeKobo.Where(g => g.Id_formulaire!.ToUpper().Equals(frm.Id.ToString().ToUpper())).ToListAsync();
                        if(grps is not null &&  grps.Count != 0)
                        {
                            foreach (var g in grps)
                            {
                                qsts = await dataContext.QuestionKobo.Where(q => q.Id_groupe!.ToUpper().Equals(g.Id.ToString().ToUpper())).ToListAsync();
                                if(qsts is not null && qsts.Count != 0)
                                {
                                    foreach (var q in qsts)
                                    {
                                        rps = await dataContext.ReponseKobo.Where(r => r.Id_question!.ToUpper().Equals(q.Id.ToString().ToUpper())).ToListAsync();
                                        if(rps is not null && rps.Count != 0)
                                        {
                                            dataContext.ReponseKobo.RemoveRange(rps);
                                        }
                                    }
                                    dataContext.QuestionKobo.RemoveRange(qsts);
                                }
                            }
                            dataContext.GroupeKobo.RemoveRange(grps);
                        }
                    }
                }
                foreach(var delete in model.NotDel!)
                {
                    model.koboForm.RemoveAll(f => f.Uid!.Equals(delete));
                }
                dataContext.FormulairesKobo.RemoveRange(model.koboForm!);
            }*/
            verification_question = new();
            if (model.IsMore)
            {
                foreach(var fr_asse in model.koboAssets!)
                {
                    Dictionary<string, List<string>> grpEtQuestion = new();
                    FormulaireKoboModel new_formulaire = new();
                    new_formulaire.Uid = fr_asse.Uid;
                    new_formulaire.Nom_formulaire = fr_asse.Name;
                    await dataContext.FormulairesKobo.AddAsync(new_formulaire);
                    var surveys = await ApiKoboToolBox.GetFormAsync(new_formulaire.Uid!);
                    System.IO.File.WriteAllText("debug_kobo_raw.json", JsonConvert.SerializeObject(surveys, Formatting.Indented));
                    var cleaned = ApiKoboToolBox.ExtractGroupsClean(surveys);
                    var reponse_complet = await ApiKoboToolBox.AllAnswer(new_formulaire.Uid!);
                    if (cleaned != null && cleaned.Count > 0)
                    {
                        foreach (var grp in cleaned)
                        {
                            int position_groupe = 1;
                            GroupeInfoModel grp_info_model = new();
                            GroupeKoboModel grp_kobo = new();
                            grp_info_model = grp.Value;
                            //KoboSoumissionModel soumissionMode = new();
                            //DateTime date_soumission = DateTime.ParseExact(reponse_complet[next_reponse].Reponse[1], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                            //DateTime date_soumission = DateTime.ParseExact(one_reponse_complet.Reponse[1], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                            //soumissionMode.Date_soumission = date_soumission;
                            //soumissionMode.Uid = new_formulaire.Uid;
                            //await dataContext.Soumissions.AddAsync(soumissionMode);
                            grp_kobo.Id_formulaire = new_formulaire.Id.ToString().ToUpper();
                            grp_kobo.Uid = new_formulaire.Uid;
                            grp_kobo.Nom_groupe = grp_info_model.Nom;
                            grp_kobo.Nom_technique = (grp_kobo.Nom_groupe == "Racine" && grp_kobo.Nom_technique == null)? "Racine" : grp_info_model.Nom_technique;
                            grp_kobo.Is_reapet = grp_info_model.IsRepeat;
                            await dataContext.GroupeKobo.AddAsync(grp_kobo);
                            List<string> id_ques_temps = new();
                            foreach (var qs in grp_info_model.Questions!)
                            {
                                QuestionKoboModel new_question = new();
                                new_question.Id = Guid.NewGuid();
                                new_question.Id_groupe = grp_kobo.Id.ToString().ToUpper();
                                new_question.Question = qs;
                                qsts!.Add(new_question);
                                id_ques_temps.Add(new_question.Id.ToString().ToUpper());
                                await dataContext.QuestionKobo.AddAsync(new_question);
                            }
                            grpEtQuestion.Add(grp_kobo.Nom_technique!, id_ques_temps);
                        }
                        foreach(var one_reponse in reponse_complet)
                        {
                            KoboSoumissionModel new_soumission = new();
                            new_soumission.Uid = new_formulaire.Uid;
                            DateTime date_soumission = DateTime.ParseExact(one_reponse.Reponse[1], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                            new_soumission.Date_soumission = date_soumission;
                            await dataContext.Soumissions.AddAsync(new_soumission);
                            List<string> id_question = new();
                            if (one_reponse.Reponse.Count > 2)
                            {
                                id_question = grpEtQuestion["Racine"];
                                if(id_question.Count == (one_reponse.Reponse.Count - 2))
                                {
                                    for(int i = 2; i < one_reponse.Reponse.Count; i++)
                                    {
                                        int id_ques = i - 2;
                                        ReponseKoboModel new_reponse = new();
                                        new_reponse.Id_soumission = new_soumission.Id.ToString().ToUpper();
                                        new_reponse.Id_question = id_question[id_ques];
                                        new_reponse.Reponse = one_reponse.Reponse[i];
                                        await dataContext.ReponseKobo.AddAsync(new_reponse);

                                    }
                                }
                            }
                            var groupeEtReponse = one_reponse.GroupeResponse;
                            foreach (var one_grp_rps in groupeEtReponse)
                            {
                                id_question = grpEtQuestion[one_grp_rps.Nom_technique!];
                                var ReponseReapet = one_grp_rps.Reponse;
                                foreach (var one_insert in ReponseReapet)
                                {
                                    if (id_question.Count == one_insert.ReponseQuestion.Count)
                                    {
                                        for (int i = 0; i < id_question.Count; i++)
                                        {
                                            ReponseKoboModel new_reponse = new();
                                            new_reponse.Id_soumission = new_soumission.Id.ToString().ToUpper();
                                            new_reponse.Id_question = id_question[i];
                                            new_reponse.Reponse = one_insert.ReponseQuestion[i];
                                            await dataContext.ReponseKobo.AddAsync(new_reponse);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                }
            }
            await dataContext.SaveChangesAsync();
            //if (model.IsMore)
            //{
            //    foreach(var form_ques in model.koboAssets!)
            //    {
            //        await InsertAnswer(form_ques.Uid!);
            //    }
            //}
            formulaires = await dataContext.FormulairesKobo.ToListAsync();
            return Ok(formulaires);
        }


        //methode controlleur
        public async Task InsertAnswer(string uid)
        {
            var cleaned = await ApiKoboToolBox.AllAnswer(uid);

            if (cleaned.Count > 0)
            {
                foreach (var grp_rps in cleaned)
                {
                    KoboSoumissionModel soumission = new();
                    soumission.Uid = uid;
                    DateTime date_soumission = DateTime.ParseExact(grp_rps.Reponse[1], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    soumission.Date_soumission = date_soumission;
                    await dataContext.Soumissions.AddAsync(soumission);
                    int pos_ques = 0;
                    if (grp_rps.Reponse.Count > 2)
                    {
                        var grp_racine = await dataContext.GroupeKobo.FirstOrDefaultAsync(g => g.Uid!.Equals(uid) && g.Nom_groupe!.Equals("Racine"));
                        if (grp_racine != null)
                        {
                            var questions = await dataContext.QuestionKobo.Where(q => q.Id_groupe!.Equals(grp_racine.Id.ToString().ToUpper())).ToListAsync();

                            
                            for (int i = 2; i < grp_rps.Reponse.Count; i++)
                            {
                                var ques = questions.FirstOrDefault(q => q.Question!.Equals(verification_question[pos_ques]));
                                ReponseKoboModel reponse = new();
                                reponse.Id_soumission = soumission.Id.ToString().ToUpper();
                                reponse.Id_question = ques!.Id.ToString().ToUpper();
                                reponse.Reponse = grp_rps.Reponse[i];
                                pos_ques++;
                                await dataContext.ReponseKobo.AddAsync(reponse);
                            }
                        }
                    }
                    if (grp_rps.GroupeResponse.Count > 0)
                    {
                        foreach (var one_grp in grp_rps.GroupeResponse)
                        {
                            var grp_bd = await dataContext.GroupeKobo.FirstOrDefaultAsync(g => g.Nom_technique!.Equals(one_grp.Nom_technique));
                            if (grp_bd != null)
                            {
                                var ques = await dataContext.QuestionKobo.Where(q => q.Id_groupe!.Equals(grp_bd.Id.ToString().ToUpper())).ToListAsync();
                                if (ques != null && ques.Count > 0)
                                {
                                    foreach (var one_soumis in one_grp.Reponse)
                                    {
                                        int temp_pos_ques = pos_ques;
                                        if (one_soumis.ReponseQuestion != null && one_soumis.ReponseQuestion.Count > 0 && ques.Count == one_soumis.ReponseQuestion.Count)
                                        {
                                            foreach (var o_q in one_soumis.ReponseQuestion)
                                            {
                                                if (verification_question[temp_pos_ques] == "0")
                                                {
                                                    temp_pos_ques++;
                                                }
                                                var one_question = ques.FirstOrDefault(q => q.Question!.Equals(verification_question[temp_pos_ques]));
                                                ReponseKoboModel reponse_one = new();
                                                reponse_one.Id_soumission = soumission.Id.ToString().ToUpper();
                                                reponse_one.Id_question = one_question!.Id.ToString().ToUpper();
                                                reponse_one.Reponse = o_q;
                                                await dataContext.ReponseKobo.AddAsync(reponse_one);
                                                temp_pos_ques++;
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

        [HttpGet("alldata/for/one/form/{uid}")]
        public async Task<IActionResult> AllDataForOneForm(string uid)
        {
            if (uid.IsNullOrEmpty())
            {
                return Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: "Requete invalide",
                        detail: "La requête est invalide ou manquant"
                    );
            }
            if (dataContext is null ||
                dataContext.FormulairesKobo is null ||
                dataContext.GroupeKobo is null ||
                dataContext.QuestionKobo is null ||
                dataContext.ReponseKobo is null ||
                dataContext.Soumissions is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur Serveur",
                        detail: "Le contexte de données est introuvable"
                    );
            }
            var formulaire = await dataContext.FormulairesKobo.FirstOrDefaultAsync(f => f.Uid!.Equals(uid));
            if(formulaire is null)
            {
                return Problem(
                            statusCode: StatusCodes.Status404NotFound,
                            title: "Formulaire non trouvée",
                            detail: "Aucun formulaire trouvée"
                        );
            }
            var soumissions = await dataContext.Soumissions.Where(s => s.Uid!.Equals(formulaire.Uid))
                                .OrderByDescending(s => s.Date_soumission).ToListAsync();
            var groupes = await dataContext.GroupeKobo.Where(g => g.Uid!.Equals(uid)).ToListAsync();
            int count = 0;
            Dictionary<string, List<ReponseKoboModel>> reponses_dico = new();
            Dictionary<string, List<QuestionKoboModel>> questions_dico = new();
            List<ReponseApiKoboHelper> reponse_dico_soummision = new();
            if(soumissions is not null && soumissions.Count > 0 && groupes is not null && groupes.Count > 0)
            {
                List<string> nom_technique_groupe = new();
                foreach(var one_soumission in soumissions)
                {
                    reponses_dico = new();
                    ReponseApiKoboHelper newreponseApi = new();
                    count++;
                    newreponseApi.NbrSoummission = count;
                    foreach(var one_grp in groupes)
                    {
                        var questions = await dataContext.QuestionKobo.Where(q => q.Id_groupe!.Equals(one_grp.Id.ToString().ToUpper())).ToListAsync();
                        if(questions is not null && questions.Count > 0)
                        {
                            if(count == 1) questions_dico.Add(one_grp.Nom_groupe!, questions);
                            foreach (var one_ques in questions)
                            {
                                var reponse = await dataContext.ReponseKobo.Where(r => r.Id_question.Equals(one_ques.Id.ToString().ToUpper())
                                            && r.Id_soumission!.Equals(one_soumission.Id.ToString().ToUpper())).ToListAsync();
                                if(reponse is not null)
                                {
                                    if(reponses_dico.TryGetValue(one_ques.Id.ToString().ToUpper(), out var list))
                                    {
                                        list.AddRange(reponse);
                                    }
                                    else
                                    {
                                        reponses_dico.Add(one_ques.Id.ToString().ToUpper(), reponse);
                                    }
                                }
                            }
                        }
                    }
                    newreponseApi.Reponse_dico = reponses_dico;
                    int taille = 0;
                    foreach(var reponse in newreponseApi.Reponse_dico)
                    {
                        if(taille < reponse.Value.Count)
                        {
                            taille = reponse.Value.Count;
                        }
                    }
                    newreponseApi.Tailler = taille;
                    reponse_dico_soummision.Add(newreponseApi);
                    
                }
            }
            ReponseForViewHelper rps = new();
            rps.Formulaire = formulaire;
            rps.Questions_dico = questions_dico;
            rps.Reponse_soumi = reponse_dico_soummision;
            return Ok(rps);
        }
        [HttpGet("kobo/synch/one/{uid}")]
        public async Task<IActionResult> SynchOneForm(string uid)
        {
            if (dataContext is null ||
                dataContext.FormulairesKobo is null ||
                dataContext.GroupeKobo is null ||
                dataContext.QuestionKobo is null ||
                dataContext.ReponseKobo is null ||
                dataContext.Soumissions is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur Serveur",
                        detail: "Le contexte de données est introuvable"
                    );
            }
            var formulaire = await dataContext.FormulairesKobo.FirstOrDefaultAsync(f => f.Uid.ToUpper().Equals(uid.ToUpper()));
            if (formulaire is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Formulaire introuvable,",
                        detail: "On n'arive pas à trouver ce fomrulaire"
                    );
            }
            var isOnline = false;
            try
            {
                await ApiKoboToolBox.GetAllProject();
                isOnline = true;
            }
            catch (HttpRequestException)
            {
                isOnline = false;
                return Ok(false);
            }
            var groupes = await dataContext.GroupeKobo.Where(g => g.Uid!.ToUpper().Equals(uid)).ToListAsync();
            if (groupes.Count > 0)
            {
                foreach (var item in groupes)
                {
                    var questions = await dataContext.QuestionKobo.Where(q => q.Id_groupe!.ToUpper().Equals(item.Id.ToString().ToUpper())).ToListAsync();
                  
                    if (questions.Count > 0)
                    {
                        foreach (var item_ques in questions)
                        {
                            var reponses = await dataContext.ReponseKobo.Where(r => r.Id_question!.ToUpper().Equals(item_ques.Id.ToString().ToUpper())).ToListAsync();
                            if (reponses.Count > 0)
                            {
                                dataContext.ReponseKobo.RemoveRange(reponses);
                            }
                        }
                        dataContext.QuestionKobo.RemoveRange(questions);
                    }
                }
                dataContext.GroupeKobo.RemoveRange(groupes);
            }
            var soummissions = await dataContext.Soumissions.Where(s => s.Uid!.Equals(uid)).ToListAsync();
            if(soummissions.Count > 0)
            {
                dataContext.Soumissions.RemoveRange(soummissions);
            }
            dataContext.FormulairesKobo.Remove(formulaire);
            await dataContext.SaveChangesAsync();
            return Ok(true);
        }
    }
}
