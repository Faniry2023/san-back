using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAN_API.Data;
using SAN_API.Helper.Vulnerabilite;
using SAN_API.Models;
using System.Threading.Tasks;

namespace SAN_API.Controllers
{
    public class VulnerabiliteController : Controller
    {
        public readonly DataContext dataContext;
        public VulnerabiliteController(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }
        [HttpGet("vulnerabilite/all")]
        public async Task<IActionResult> GetAll()
        {
            if (dataContext is null || dataContext?.Enquetes is null || dataContext?.Evenements is null || dataContext?.Val_mensuels is null || dataContext?.Relations is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }
            var val_mensuel = await dataContext.Val_mensuels.ToListAsync();
            if(val_mensuel is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Aucun Valeur",
                        detail: "Aucun Valeur n'a été trouvé"
                );
            }
            List<VulnerabiliteHelper> lvhs = new();
            foreach(var item in val_mensuel)
            {
                var evenement = await dataContext.Evenements.FirstOrDefaultAsync(e => e.Id_val_mens!.ToUpper().Equals(item.Id.ToString().ToUpper()));
                if(evenement is null)
                {
                    return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Aucun événement",
                        detail: "Aucun événement n'a été trouvé"
                    );
                }
                var enquete = await dataContext.Enquetes.FirstOrDefaultAsync(e => e.Id.ToString().ToUpper().Equals(evenement.Id_enquete!.ToUpper()));
                var relation = await dataContext.Relations.FirstOrDefaultAsync(r => r.Id_val_mensuel!.ToUpper().Equals(item.Id.ToString().ToUpper()));
                var es = await dataContext.Es.FirstOrDefaultAsync(e => e.Id.ToString().ToUpper().Equals(item.Id_es!.ToUpper()));
                var gadm = await dataContext.Gadm.FirstOrDefaultAsync(g => g.Id!.ToString().ToUpper().Equals(item.Id_gadm!.ToUpper()));
                var temps = await dataContext.Temps.FirstOrDefaultAsync(t => t.Id!.ToString().ToUpper().Equals(item.Id_temps!.ToUpper()));
                if (enquete is null)
                {
                    return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Aucun enquête",
                        detail: "Aucun enquête n'a été trouvé"
                    );
                }
                if (es is null)
                {
                    return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Aucun Es",
                        detail: "Aucun Es n'a été trouvé"
                    );
                }
                VulnerabiliteHelper vh = new();
                vh.Val_mensuel = item;
                vh.Enquete = enquete;
                vh.Evenement = evenement;
                vh.Relation = relation;
                vh.Es = es;
                vh.Gadm = gadm;
                vh.Temps = temps;
                lvhs.Add( vh );
            }
            return Ok(lvhs);
        }

        [HttpPost("vulnerabilite/add")]
        public async Task<IActionResult> Add([FromBody]PostVulnerabiliteHelper model)
        {
            if (model is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Requete invalide",
                    detail: "La requête est invalide ou manquant"
                );
            }
            if (dataContext is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }
            TempsModel newTemps = new();
            newTemps.Nom_evenement = model.Temps!.Nom_evenement;
            newTemps.Annee = model.Temps.Annee;
            newTemps.Jan = model.Temps?.Jan;
            newTemps.Fev = model.Temps?.Fev;
            newTemps.Mar = model.Temps?.Mar;
            newTemps.Avr = model.Temps?.Avr;
            newTemps.Mai = model.Temps?.Mar;
            newTemps.Jui = model.Temps?.Jui;
            newTemps.Juill = model.Temps?.Juill;
            newTemps.Aou = model.Temps?.Aou;
            newTemps.Sep = model.Temps?.Sep;
            newTemps.Oct = model.Temps?.Oct;
            newTemps.Nov = model.Temps?.Nov;
            newTemps.Dec = model.Temps?.Dec;

            await dataContext.Temps.AddAsync(newTemps);
            Val_mensuelModel newVal = new();
            newVal.Id_temps = newTemps.Id.ToString().ToUpper();
            newVal.Id_gadm = model.Gadm!.Id;
            newVal.Nom_val = model.Val_mensuel!.Nom_val;
            newVal.Id_es = model.Es!.Id!.ToUpper();
            await dataContext.Val_mensuels.AddAsync(newVal);
            EvenementModel newEvenement = new();
            newEvenement.Id_enquete = model.Evenement!.Id_enquete!.ToUpper();
            newEvenement.Id_val_mens = newVal.Id.ToString().ToUpper();
            newEvenement.Cle_principal = model.Evenement!.Cle_principal;
            await dataContext.Evenements.AddAsync(newEvenement);

            await dataContext.SaveChangesAsync();
            VulnerabiliteHelper newVul = new();
            EnqueteModel enquete = new();
            enquete.Id = Guid.Parse(model.Enquete!.Id!);
            enquete.Nom = model.Enquete.Nom;
            RelationModel rlm = new();
            ESModel es = new();
            es.Id = Guid.Parse(model.Es.Id);
            es.Nom = model.Es.Nom;
            es.Prenom = model.Es.Prenom;
            es.Id_gadm = model.Es.Id_gadm;
            es.Nb_site = model.Es.Nb_site;
            es.Telephone = model.Es.Telephone;
            
            newVul.Enquete = enquete;
            newVul.Evenement = newEvenement;
            newVul.Val_mensuel = newVal;
            newVul.Relation = rlm;
            newVul.Gadm = model.Gadm;
            newVul.Temps = newTemps;
            newVul.Es = es;

            return Ok(newVul);
        }

        [HttpPut("vulnerabilite/update")]
        public async Task<IActionResult> Update([FromBody]UpdateVulnerabiliteHelper model)
        {
            if (model is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Requete invalide",
                    detail: "La requête est invalide ou manquant"
                );
            }
            if (dataContext is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }
            var update_val_men = await dataContext.Val_mensuels.FirstOrDefaultAsync(v => v.Id.ToString().ToUpper().Equals(model.Val_mensuel!.Id!.ToUpper()));
            if (update_val_men is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Aucun valeur mensuel",
                    detail: "Aucun valeur mensuel n'a été trouvé"
                );
            }
            update_val_men.Id_temps = model.Val_mensuel!.Id_temps!.ToUpper();
            update_val_men.Id_gadm = model.Val_mensuel.Id_gadm!.ToUpper();
            update_val_men.Nom_val = model.Val_mensuel.Nom_val;
            update_val_men.Id_es = model.Val_mensuel.Id_es!.ToUpper();

            var update_evenement = await dataContext.Evenements.FirstOrDefaultAsync(e => e.Id.ToString().ToUpper().Equals(model.Evenement!.Id!.ToUpper()));
            if (update_evenement is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Aucun evenement",
                    detail: "Aucun evenement n'a été trouvé"
                );
            }
            update_evenement.Id_enquete = model.Evenement!.Id_enquete!.ToUpper();
            update_evenement.Cle_principal = model.Evenement.Cle_principal;

            var temps = await dataContext.Temps.FirstOrDefaultAsync(t => t.Id.ToString().ToUpper().Equals(model.Temps!.Id!.ToUpper()));
            if (temps is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Aucun Temps",
                    detail: "Aucun Temps n'a été trouvé"
                );
            }
            temps.Nom_evenement = model.Temps!.Nom_evenement;
            temps.Jan = model.Temps.Jan;
            temps.Fev = model.Temps.Fev;
            temps.Mar = model.Temps.Mar;
            temps.Avr = model.Temps.Avr;
            temps.Mai = model.Temps.Mai;
            temps.Jui = model.Temps.Jui;
            temps.Juill = model.Temps.Juill;
            temps.Aou = model.Temps.Aou;
            temps.Sep = model.Temps.Sep;
            temps.Oct = model.Temps.Oct;
            temps.Nov = model.Temps.Nov;
            temps.Dec = model.Temps.Dec;

            await dataContext.SaveChangesAsync();

            var enquete = await dataContext.Enquetes.FirstOrDefaultAsync(e => e.Id.ToString().ToUpper().Equals(model.Evenement.Id_enquete.ToUpper()));
            var relation = await dataContext.Relations.FirstOrDefaultAsync(r => r.Id_val_mensuel!.ToUpper().Equals(model.Val_mensuel.Id!.ToUpper()));
            var es = await dataContext.Es.FirstOrDefaultAsync(e => e.Id.ToString().ToUpper().Equals(model.Val_mensuel.Id_es.ToUpper()));
            var gadm = await dataContext.Gadm.FirstOrDefaultAsync(g => g.Id!.ToUpper().Equals(model.Val_mensuel.Id_gadm.ToUpper()));
            if (enquete is null || es is null || gadm is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Objet incomplet",
                    detail: "Un élément manquant dans l'objet"
                );
            }
            VulnerabiliteHelper updateVul = new();
            updateVul.Enquete = enquete;
            updateVul.Evenement = update_evenement;
            updateVul.Val_mensuel = update_val_men;
            updateVul.Relation = relation;
            updateVul.Es = es;
            updateVul.Gadm = gadm;
            updateVul.Temps = temps;

            return Ok(updateVul);
        }
        [HttpDelete("vulnerabilite/delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Requete invalide",
                    detail: "La requête est invalide ou manquant"
                );
            }
            if (dataContext is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }

            var evenement = await dataContext.Evenements.FirstOrDefaultAsync(e => e.Id.ToString().ToUpper().Equals(id.ToUpper()));
            if (evenement is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Aucun Evenement",
                    detail: "Aucun evenement n'a été trouvé"
                );
            }
            var val_mensuel = await dataContext.Val_mensuels.FirstOrDefaultAsync(v => v.Id.ToString().ToUpper().Equals(evenement.Id_val_mens!.ToUpper()));
            if (val_mensuel is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Aucun val_mensuel",
                    detail: "Aucun val_mensuel n'a été trouvé"
                );
            }
            var temps = await dataContext.Temps.FirstOrDefaultAsync(t => t.Id.ToString().ToUpper().Equals(val_mensuel.Id_temps!.ToUpper()));
            if (temps is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Aucun Temps",
                    detail: "Aucun Temps n'a été trouvé"
                );
            }
            dataContext.Evenements.Remove(evenement);
            dataContext.Val_mensuels.Remove(val_mensuel);
            dataContext.Temps.Remove(temps);
            await dataContext.SaveChangesAsync();
            return Ok();
        }
    }
}
