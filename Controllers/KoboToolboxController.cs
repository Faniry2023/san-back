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
using System.Linq;
using System.Threading.Tasks;

namespace SAN_API.Controllers
{
    public class KoboToolboxController : Controller
    {
        public readonly DataContext dataContext;
        public KoboToolboxController(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }
        [HttpGet("kobo/projetcts")]
        public async Task<IActionResult> GetAllProjects()
        {
            if(dataContext is null || 
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
            List<FormulaireKoboModel> liste_formulaires = new();
            liste_formulaires = await dataContext.FormulairesKobo.ToListAsync();
            if(liste_formulaires is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Formulaire non trouvée",
                        detail: "Aucun formulaire trouvée"
                    );
            }

            List<KoboAssetModel> liste_assets = new();
            liste_assets = await ApiKoboToolBox.GetAllProject();
            
            if(liste_assets.Count != liste_formulaires.Count)
            {
                RetoureKoboFormulaireHelper retoureKoboFormulaire = new();
                if (liste_assets.Count > liste_formulaires.Count)
                {
                    retoureKoboFormulaire.IsMore = true                                                                                                                                                                                                                                                                  ;
                    retoureKoboFormulaire.IsLess = false;
                    retoureKoboFormulaire.Alert = false;
                    var set = new HashSet<string>(liste_formulaires.Select(f => f.Uid!));
                    int stop = 1;
                    foreach(var lst_asst in liste_assets)
                    {
                        if (!set.Contains(lst_asst.Uid!))
                        {
                            if (stop == liste_assets.Count - 1)
                            {
                                break;
                            }
                            retoureKoboFormulaire.koboAssets!.Add(lst_asst);
                            stop++;
                        }
                    }
                    retoureKoboFormulaire.CountNew = retoureKoboFormulaire.koboAssets!.Count;
                    
                }
                if(liste_assets.Count < liste_formulaires.Count)
                {
                    retoureKoboFormulaire.IsMore = false;
                    retoureKoboFormulaire.IsLess = true;
                    retoureKoboFormulaire.Alert = true;
                    var set = new HashSet<string>(liste_assets.Select(f => f.Uid!));
                    foreach(var ls_form in liste_formulaires)
                    {
                        
                        if (!set.Contains(ls_form.Uid!))
                        {
                            retoureKoboFormulaire.koboForm!.Add(ls_form);
                        }
                    }
                    retoureKoboFormulaire.CountNew = retoureKoboFormulaire.koboForm!.Count;
                }
                return Ok(retoureKoboFormulaire);
            }

            return Ok(liste_formulaires);
        }

        //Nouvelle formulaire
        [HttpGet("kobo/new/synchronisation")]
        public async Task<IActionResult> NewSynchronisation(RetoureKoboFormulaireHelper model)
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
            if (model.IsLess)
            {
                foreach(var frm in model.koboForm!)
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
                dataContext.FormulairesKobo.RemoveRange(model.koboForm!);
            }

            if (model.IsMore)
            {
                foreach(var fr_asse in model.koboAssets!)
                {
                    FormulaireKoboModel new_formulaire = new();
                    new_formulaire.Uid = fr_asse.Uid;
                    new_formulaire.Nom_formulaire = fr_asse.Name;
                    await dataContext.FormulairesKobo.AddAsync(new_formulaire);
                    var surveys = await ApiKoboToolBox.GetFormAsync(new_formulaire.Uid!);
                    System.IO.File.WriteAllText("debug_kobo_raw.json", JsonConvert.SerializeObject(surveys, Formatting.Indented));
                    var cleaned = ApiKoboToolBox.ExtractGroupsClean(surveys);
                    if(cleaned != null && cleaned.Count > 0)
                    {
                        foreach (var grp in cleaned)
                        {
                            GroupeInfoModel grp_info_model = new();
                            GroupeKoboModel grp_kobo = new();
                            grp_info_model = grp.Value;

                            grp_kobo.Id_formulaire = new_formulaire.Id.ToString().ToUpper();
                            grp_kobo.Uid = new_formulaire.Uid;
                            grp_kobo.Nom_groupe = grp_info_model.Nom;
                            grp_kobo.Nom_technique = grp_info_model.Nom_technique;
                            grp_kobo.Is_reapet = grp_info_model.IsRepeat;
                            await dataContext.GroupeKobo.AddAsync(grp_kobo);
                            foreach(var qs in grp_info_model.Questions!)
                            {
                                QuestionKoboModel new_question = new();
                                new_question.Id = Guid.NewGuid();
                                new_question.Id_groupe = grp_kobo.Id.ToString().ToUpper();
                                new_question.Question = qs;
                                qsts!.Add(new_question);
                            }
                            await dataContext.QuestionKobo.AddRangeAsync(qsts!);
                        }
                    }
                }
            }
            await dataContext.SaveChangesAsync();
            formulaires = await dataContext.FormulairesKobo.ToListAsync();
            return Ok(formulaires);
        }
    }
}
