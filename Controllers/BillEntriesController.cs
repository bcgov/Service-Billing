using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Service_Billing.Data;
using Service_Billing.Models;

namespace Service_Billing
{
    public class BillEntriesController : Controller
    {
        private readonly ServiceBillingContext _context;

        public BillEntriesController(ServiceBillingContext context)
        {
            _context = context;
        }

        // GET: BillEntries
        public async Task<IActionResult> Index()
        {
            return View();
              //return _context.billingData != null ? 
              //            View(await _context.billingData.ToListAsync()) :
              //            Problem("Entity set 'Service_BillingContext.BillEntries'  is null.");
        }

        // GET: BillEntries/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            return NotFound();
            //if (id == null || _context.billingData == null)
            //{
            //    return NotFound();
            //}

            //var billEntries = await _context.billingData
            //    .FirstOrDefaultAsync(m => m.Id == id);
            //if (billEntries == null)
            //{
            //    return NotFound();
            //}

            //return View(billEntries);
        }

        // GET: BillEntries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BillEntries/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id")] BillEntries billEntries)
        {
            if (ModelState.IsValid)
            {
                _context.Add(billEntries);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(billEntries);
        }

        // GET: BillEntries/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            return NotFound(ModelState);
            //if (id == null || _context.billingData == null)
            //{
            //    return NotFound();
            //}

            //var billEntries = await _context.billingData.FindAsync(id);
            //if (billEntries == null)
            //{
            //    return NotFound();
            //}
            //return View(billEntries);
        }

        // POST: BillEntries/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id")] BillEntries billEntries)
        //{
        //    if (id != billEntries.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(billEntries);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!BillEntriesExists(billEntries.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(billEntries);
        //}

        // GET: BillEntries/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _context.billingData == null)
        //    {
        //        return NotFound();
        //    }

        //    var billEntries = await _context.billingData
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (billEntries == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(billEntries);
        //}

        //// POST: BillEntries/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.billingData == null)
        //    {
        //        return Problem("Entity set 'Service_BillingContext.BillEntries'  is null.");
        //    }
        //    var billEntries = await _context.billingData.FindAsync(id);
        //    if (billEntries != null)
        //    {
        //        _context.billingData.Remove(billEntries);
        //    }
            
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool BillEntriesExists(int id)
        //{
        //  return (_context.billingData?.Any(e => e.Id == id)).GetValueOrDefault();
        //}
    }
}
