using BrightIdeasSoftware;
using Nats.Services.Core.KeyValueStoreService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StoreClient
{
    public class StoreClientModel<T_Key, T_Value, T_RowItem> 
        where T_Value : IKeyIdentifiable<T_Key>
        where T_RowItem : class, StoreRowItem<T_Value>, new()
    {
        IKeyValueStoreService<T_Key, T_Value> service;

        Dictionary<T_Key, T_RowItem> dicoKeyRows = new Dictionary<T_Key, T_RowItem>();
        Dictionary<T_Key, T_RowItem> updatedRows = new Dictionary<T_Key, T_RowItem>();
        Timer timer;
        ObjectListView view;

        public async Task InitAsync(IKeyValueStoreService<T_Key, T_Value> service, ObjectListView view)
        {
            this.service = service;
            this.view = view;

            var items= await Task.Run(() => service.GetAllValues());
            foreach(var item in items)
            {
                var rowItem = new T_RowItem();
                rowItem.Update(item);
                dicoKeyRows[item.Key] = rowItem;
            }
            Generator.GenerateColumns(view, typeof(T_RowItem));

            view.Objects = dicoKeyRows.Values;
            timer = new Timer();
            timer.Tick += UpdateBatch;
            timer.Start();
            service.ValueUpdated += OnValueUpdated;
        }

        private void UpdateBatch(object sender, EventArgs evt)
        {
            List<T_RowItem> values;
            lock (updatedRows)
            {
                if(updatedRows.Count == 0)
                {
                    return;
                }
                values = updatedRows.Values.ToList();
                updatedRows.Clear();
            }
            view.Invoke( (Action)(() => RefreshRows(values)));
        }

        Stopwatch sw = new Stopwatch();
        long n = 0;
        private void RefreshRows(List<T_RowItem> values)
        {
            n++;
            sw.Start();
            view.RefreshObjects(values);
            sw.Stop();
            if( n % 10 == 0)
            {
                Debug.WriteLine("n: " + n + ", t=" + sw.ElapsedMilliseconds + ", avg: " + (sw.ElapsedMilliseconds/ (double)n));
            }
        }

        private void OnValueUpdated(T_Value obj)
        {
            lock(updatedRows)
            {
                T_RowItem rowItem; 
                if(! dicoKeyRows.TryGetValue(obj.Key, out rowItem))
                {
                    rowItem = new T_RowItem();
                    dicoKeyRows[obj.Key] = rowItem;
                }

                rowItem.Update(obj);
                updatedRows[obj.Key] = rowItem;
            }
        }
    }
}
