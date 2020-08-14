using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;
using Wareways.PortalProv.Filters;
using Wareways.PortalProv.Infraestructura;

namespace Wareways.PortalProv.Controllers.Api
{
    
    public class EmpresasController : ApiController
    {
        private PortalProvEntities db = new PortalProvEntities();

        // GET: api/Empresas
        public IQueryable<V_PPROV_Empresas> GetV_PPROV_Empresas()
        {
            ClaimsPrincipal principal = Request.GetRequestContext().Principal as ClaimsPrincipal;
            var Name = ClaimsPrincipal.Current.Identity.Name;
            return db.V_PPROV_Empresas;
        }

        // GET: api/Empresas/5
        [ResponseType(typeof(V_PPROV_Empresas))]
        public IHttpActionResult GetV_PPROV_Empresas(long id)
        {
            V_PPROV_Empresas v_PPROV_Empresas = db.V_PPROV_Empresas.Find(id);
            if (v_PPROV_Empresas == null)
            {
                return NotFound();
            }

            return Ok(v_PPROV_Empresas);
        }

        // PUT: api/Empresas/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutV_PPROV_Empresas(long id, V_PPROV_Empresas v_PPROV_Empresas)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != v_PPROV_Empresas.RowNbr)
            {
                return BadRequest();
            }

            db.Entry(v_PPROV_Empresas).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!V_PPROV_EmpresasExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Empresas
        [ResponseType(typeof(V_PPROV_Empresas))]
        public IHttpActionResult PostV_PPROV_Empresas(V_PPROV_Empresas v_PPROV_Empresas)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.V_PPROV_Empresas.Add(v_PPROV_Empresas);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                if (V_PPROV_EmpresasExists(v_PPROV_Empresas.RowNbr))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = v_PPROV_Empresas.RowNbr }, v_PPROV_Empresas);
        }

        // DELETE: api/Empresas/5
        [ResponseType(typeof(V_PPROV_Empresas))]
        public IHttpActionResult DeleteV_PPROV_Empresas(long id)
        {
            V_PPROV_Empresas v_PPROV_Empresas = db.V_PPROV_Empresas.Find(id);
            if (v_PPROV_Empresas == null)
            {
                return NotFound();
            }

            db.V_PPROV_Empresas.Remove(v_PPROV_Empresas);
            db.SaveChanges();

            return Ok(v_PPROV_Empresas);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool V_PPROV_EmpresasExists(long id)
        {
            return db.V_PPROV_Empresas.Count(e => e.RowNbr == id) > 0;
        }
    }
}