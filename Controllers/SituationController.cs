using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SAN_API.Data;
using SAN_API.Helper;
using SAN_API.Models;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace SAN_API.Controllers
{
    public class SituationController : Controller
    {
        private readonly DataContext dataContext;
        public SituationController(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }
        [HttpPost("situation/add")]
        public async Task<IActionResult> Add([FromBody]ListeSituationHelper model)
        {
            if (model is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Requete invalide",
                    detail: "La requête est invalide ou manquant"
                );
            }
            if (dataContext is null || dataContext?.Prosuits is null || dataContext?.Temps is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }
            List<CompletSituationHelper> returnList = new();
            foreach (var item in model.Situations)
            {
                TempsModel newTemps = new();
                newTemps.Nom_evenement = item.Temps!.Nom_evenement;
                newTemps.Annee = item.Temps!.Annee;
                newTemps.Jan = item.Temps!.Jan;
                newTemps.Fev = item.Temps!.Fev;
                newTemps.Mar = item.Temps!.Mar;
                newTemps.Avr = item.Temps!.Avr;
                newTemps.Mai = item.Temps!.Mai;
                newTemps.Jui = item.Temps!.Jui;
                newTemps.Juill = item.Temps!.Juill;
                newTemps.Aou = item.Temps!.Aou;
                newTemps.Sep = item.Temps!.Sep;
                newTemps.Oct = item.Temps!.Oct;
                newTemps.Nov = item.Temps!.Nov;
                newTemps.Dec = item.Temps!.Dec;
                await dataContext.Temps.AddAsync(newTemps);
                ProduitModel newProduit = new();
                newProduit.Id_temps = newTemps.Id.ToString().ToUpper();
                newProduit.Id_gadm = item.Produit!.Id_gadm!.ToUpper();
                newProduit.Id_enquete = item.Produit!.Id_enquete!.ToUpper();
                newProduit.Id_es = item.Produit!.Id_es!.ToUpper();
                newProduit.Nom_prod = item.Produit!.Nom_prod;
                newProduit.Unite = item.Produit!.Unite;
                newProduit.Cle = item.Produit!.Cle;
                await dataContext.Prosuits.AddAsync(newProduit);

                //corréction helper

                CompletSituationHelper newSituation = new();
                newSituation.Produit = newProduit;
                newSituation.Temps = newTemps;
                ESModel esModel = new();
                var es = await dataContext.Es.FirstOrDefaultAsync(e => e.Id.ToString().ToUpper().Equals(newProduit.Id_es));
                var enquete = await dataContext.Enquetes.FirstOrDefaultAsync(e => e.Id.ToString().ToUpper().Equals(newProduit.Id_enquete));
                var y_gadm = await dataContext.Gadm.FirstOrDefaultAsync(g => g.Id.ToString().ToUpper().Equals(newProduit.Id_gadm));
                if(es is null && enquete is null && y_gadm is null)
                {
                    return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Un élément introuvable",
                        detail: "Aucun aucun élément ne correspond à cet id"
                    );
                }
                newSituation.Es = es;
                newSituation.Enquete = enquete;
                newSituation.Gadm = y_gadm;

                await dataContext.SaveChangesAsync();

                returnList.Add(newSituation);
            }

            return Ok(returnList);
        }

        [HttpDelete("situation/delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id.IsNullOrEmpty())
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Requete invalide",
                    detail: "La requête est invalide ou manquant"
                );
            }
            if (dataContext is null || dataContext?.Prosuits is null || dataContext?.Temps is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }

            var produit = await dataContext.Prosuits.FirstOrDefaultAsync(p => p.Id!.ToString().ToUpper().Equals(id.ToUpper()));
            if(produit == null)
            {
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Produit introuvable",
                        detail: "Aucun produit ne correspond à cet id"
                    );
            }
            dataContext.Prosuits.Remove(produit);
            var temps = await dataContext.Temps.FirstOrDefaultAsync(t => t.Id.ToString().ToUpper().Equals(produit.Id_temps));

            
            if (temps == null)
            {
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Temps introuvable",
                        detail: "Aucun temps ne correspond à cet id"
                    );
            }
            var dispo = await dataContext.Disponibilites.FirstOrDefaultAsync(d => d.Id_prod!.ToUpper().Equals(produit.Id.ToString().ToUpper()));
            if (dispo == null)
            {
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "dispo introuvable",
                        detail: "Aucun dispo ne correspond à cet id"
                    );
            }
            var temps_dis = await dataContext.Temps.FirstOrDefaultAsync(t => t.Id.ToString().ToUpper().Equals(dispo.Id_temps!.ToUpper()));
            if(temps_dis == null)
            {
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Temps introuvable",
                        detail: "Aucun temps ne correspond à cet id"
                    );
            }
            dataContext.Disponibilites.Remove(dispo);
            dataContext.Temps.Remove(temps_dis);
            dataContext.Temps.Remove(temps);

            return Ok();
        }

        [HttpGet("situation/getall")]
        public async Task<IActionResult> GetAll()
        {
            if (dataContext is null || dataContext?.Disponibilites is null || dataContext?.Prosuits is null || dataContext?.Temps is null || dataContext?.Es is null || dataContext?.Gadm is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }
            var produits = await dataContext.Prosuits.ToListAsync();
            if (produits is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Aucun Produit",
                        detail: "Aucun produit n'a été trouvé"
                );
            }
            List<CompletSituationHelper> list_csh = new();
            foreach(var item in produits)
            {
                var gadm = await dataContext.Gadm.FirstOrDefaultAsync(g => g.Id!.ToString().ToUpper().Equals(item.Id_gadm));
                var temps = await dataContext.Temps.FirstOrDefaultAsync(t => t.Id.ToString().ToUpper().Equals(item.Id_temps));
                var enquete = await dataContext.Enquetes.FirstOrDefaultAsync(e => e.Id.ToString().ToUpper().Equals(item.Id_enquete));
                var es = await dataContext.Es.FirstOrDefaultAsync(e => e.Id.ToString().ToUpper().Equals(item.Id_es));
                var dis = await dataContext.Disponibilites.FirstOrDefaultAsync(d => d.Id_prod!.ToUpper().Equals(item.Id.ToString().ToUpper()));
                CompletSituationHelper csh = new();
                if (gadm == null || temps == null || enquete == null || es == null)
                {
                    return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Un élément manquant",
                        detail: "Un élément du produit est manquant (gadm/temps/enquete/es)"
                    );
                }
                
                if (dis != null)
                {
                    var temps_dis = await dataContext.Temps.FirstOrDefaultAsync(t => t.Id.ToString().ToUpper().Equals(dis.Id_temps!.ToUpper()));
                    if(temps_dis != null)
                    {
                        var disHelper = new DisponibiliteHelper
                        {
                            Id = dis.Id.ToString(),
                            Id_prod = dis.Id_prod,
                            Temps = new TempsHelper
                            {
                                Id = temps_dis!.Id.ToString(),
                                Nom_evenement = temps_dis.Nom_evenement,
                                Jan = temps_dis.Jan,
                                Fev = temps_dis.Fev,
                                Mar = temps_dis.Mar,
                                Avr = temps_dis.Avr,
                                Mai = temps_dis.Mai,
                                Jui = temps_dis.Jui,
                                Juill = temps_dis.Juill,
                                Aou = temps_dis.Aou,
                                Sep = temps_dis.Sep,
                                Oct = temps_dis.Oct,
                                Nov = temps_dis.Nov,
                                Dec = temps_dis.Dec,
                            }
                        };

                        csh.Disponibilite = disHelper;
                    }
                    else
                    {
                        return Problem(
                            statusCode: StatusCodes.Status404NotFound,
                            title: "Un élément manquant",
                            detail: "Un élément du produit est manquant (temps_dis)"
                        );
                    }
                }
                csh.Produit = item;
                csh.Gadm = gadm;
                csh.Temps = temps;
                csh.Es = es;
                csh.Enquete = enquete;
                
                list_csh.Add(csh);

            }
            return Ok(list_csh);
        }

        [HttpPut("situation/update")]
        public async Task<IActionResult> Update([FromBody] UpdateSituationHelper model)
        {
            if (model is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Requete invalide",
                    detail: "La requête est invalide ou manquant"
                );
            }
            if (dataContext is null || dataContext?.Prosuits is null || dataContext?.Temps is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }
            var produit = await dataContext.Prosuits.FirstOrDefaultAsync(p => p.Id.ToString().ToUpper().Equals(model.Produit!.Id.ToString().ToUpper()));
            if(produit == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Produit introuvable",
                    detail: "Aucun produit trouvée"
                );
            }
            var temps = await dataContext.Temps.FirstOrDefaultAsync(t => t.Id.ToString().ToUpper().Equals(model.Temps!.Id.ToString().ToUpper()));
            if (temps == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Temps introuvable",
                    detail: "Aucun temps trouvée"
                );
            }
            produit.Id_gadm = model.Produit!.Id_gadm;
            produit.Id_enquete = model.Produit!.Id_enquete;
            produit.Id_es = model.Produit!.Id_es;
            produit.Nom_prod = model.Produit!.Nom_prod;
            produit.Unite = model.Produit!.Unite;
            produit.Cle = model.Produit!.Cle;

            temps.Nom_evenement = model.Temps!.Nom_evenement;
            temps.Annee = model.Temps!.Annee;
            temps.Jan = model.Temps!.Jan;
            temps.Fev = model.Temps!.Fev;
            temps.Mar = model.Temps!.Mar;
            temps.Avr = model.Temps!.Avr;
            temps.Mai = model.Temps!.Mai;
            temps.Jui = model.Temps!.Jui;
            temps.Juill = model.Temps!.Juill;
            temps.Aou = model.Temps!.Aou;
            temps.Sep = model.Temps!.Sep;
            temps.Oct = model.Temps!.Oct;
            temps.Nov = model.Temps!.Nov;
            temps.Dec = model.Temps!.Dec;

            var enquete = await dataContext.Enquetes.FirstOrDefaultAsync(e => e.Id.ToString().ToUpper().Equals(model.Produit.Id_enquete!.ToUpper()));
            var gadm = await dataContext.Gadm.FirstOrDefaultAsync(g => g.Id!.ToString().ToUpper().Equals(model.Produit.Id_gadm!.ToUpper()));
            var es = await dataContext.Es.FirstOrDefaultAsync(e => e.Id.ToString().ToUpper().Equals(model.Produit.Id_es!.ToUpper()));
            CompletSituationHelper csh = new();
            csh.Produit = produit;
            csh.Temps = temps;
            csh.Gadm = gadm;
            csh.Es = es;
            csh.Enquete = enquete;
            return Ok(csh);
        }
        [HttpPost("situation/add/disp")]
        public async Task<IActionResult> AddDispo([FromBody]DisponibiliteHelper model)
        {
            if (model is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Requete invalide",
                    detail: "La requête est invalide ou manquant"
                );
            }
            if (dataContext is null || dataContext?.Disponibilites is null || dataContext?.Temps is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }
            TempsModel new_temps = new();
            new_temps.Nom_evenement = "Disponibilité";
            new_temps.Annee = model.Temps!.Annee;
            new_temps.Jan = model.Temps.Jan;
            new_temps.Fev = model.Temps.Fev;
            new_temps.Mar = model.Temps.Mar;
            new_temps.Avr = model.Temps.Avr;
            new_temps.Mai = model.Temps.Mai;
            new_temps.Jui = model.Temps.Jui;
            new_temps.Juill = model.Temps.Juill;
            new_temps.Aou = model.Temps.Aou;
            new_temps.Sep = model.Temps.Sep;
            new_temps.Oct = model.Temps.Oct;
            new_temps.Nov = model.Temps.Nov;
            new_temps.Dec = model.Temps.Dec;

            await dataContext.Temps.AddAsync(new_temps);

            DisponibiliteModel dispo = new();
            dispo.Id_prod = model.Id_prod;
            dispo.Id_temps = new_temps.Id.ToString().ToUpper();

            await dataContext.Disponibilites.AddAsync(dispo);
            model.Id = dispo.Id.ToString();
            model.Temps.Id = new_temps.Id.ToString();
            await dataContext.SaveChangesAsync();

            var prod = await dataContext.Prosuits.FirstOrDefaultAsync(p => p.Id.ToString().ToUpper().Equals(model.Id_prod!.ToUpper()));
            if (prod == null )
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Un élément manquant",
                    detail: "Un élément du produit est manquant"
                );
            }
            var gadm = await dataContext.Gadm.FirstOrDefaultAsync(g => g.Id!.ToString().ToUpper().Equals(prod.Id_gadm!.ToUpper()));
            var enquete = await dataContext.Enquetes.FirstOrDefaultAsync(e => e.Id.ToString().ToUpper().Equals(prod.Id_enquete!.ToUpper()));
            var es = await dataContext.Es.FirstOrDefaultAsync(e => e.Id.ToString().ToUpper().Equals(prod.Id_es!.ToUpper()));
            if (gadm == null || enquete == null || es == null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Un élément manquant",
                    detail: "Un élément du produit est manquant"
                );
            }
            CompletSituationHelper csh = new();
            csh.Produit = prod;
            csh.Gadm = gadm;
            csh.Temps = new_temps;
            csh.Es = es;
            csh.Enquete = enquete;
            csh.Disponibilite = model;
            return Ok(csh);
        }
        [HttpPut("situation/update/dispo")]
        public async Task<IActionResult> UpdateDispo([FromBody] DisponibiliteHelper model)
        {
            if (model is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Requete invalide",
                    detail: "La requête est invalide ou manquant"
                );
            }
            if (dataContext is null || dataContext?.Disponibilites is null || dataContext?.Temps is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }
            var temps_dispo = await dataContext.Temps.FirstOrDefaultAsync(t => t.Id.ToString().ToUpper().Equals(model.Temps!.Id!.ToUpper()));
            if(temps_dispo == null)
            {
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Temps introuvable",
                        detail: "Aucun temps ne correspond a cette id"
                );
            }
            temps_dispo.Jan = model.Temps!.Jan;
            temps_dispo.Fev = model.Temps.Fev;
            temps_dispo.Mar = model.Temps.Mar;
            temps_dispo.Avr = model.Temps.Avr;
            temps_dispo.Mai = model.Temps.Mai;
            temps_dispo.Jui = model.Temps.Jui;
            temps_dispo.Juill = model.Temps.Juill;
            temps_dispo.Aou = model.Temps.Aou;
            temps_dispo.Sep = model.Temps.Sep;
            temps_dispo.Oct = model.Temps.Oct;
            temps_dispo.Nov = model.Temps.Nov;
            temps_dispo.Dec = model.Temps.Dec;

            await dataContext.SaveChangesAsync();
            DisponibiliteHelper dis_hel = new();
            dis_hel.Id = model.Id;
            dis_hel.Id_prod = model.Id_prod;
            dis_hel.Temps = model.Temps;

            return Ok(dis_hel);
        }

        [HttpDelete("situation/delete/dispo/{id}")]
        public async Task<IActionResult> DeleteDispo(string id)
        {
            if (id is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Requete invalide",
                    detail: "La requête est invalide ou manquant"
                );
            }
            if (dataContext is null || dataContext?.Disponibilites is null || dataContext?.Temps is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }

            var dispo = await dataContext.Disponibilites.FirstOrDefaultAsync(d => d.Id.ToString().ToUpper().Equals(id.ToUpper()));
            if(dispo == null)
            {
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "dispo introuvable",
                        detail: "Aucun dispo ne correspond a cette id"
                );
            }
            var temps = await dataContext.Temps.FirstOrDefaultAsync(t => t.Id.ToString().ToUpper().Equals(dispo.Id_temps!.ToUpper()));
            if (temps == null)
            {
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "temps introuvable",
                        detail: "Aucun temps ne correspond a cette id"
                );
            }

            dataContext.Disponibilites.Remove(dispo);
            dataContext.Temps.Remove(temps);

            await dataContext.SaveChangesAsync();
            return Ok();
        }
    }

}
