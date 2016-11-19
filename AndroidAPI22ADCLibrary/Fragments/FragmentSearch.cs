using System;
using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using SupportFragment = Android.Support.V4.App.Fragment;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Content.Res;
using AndroidAPI22ADCLibrary.Helpers;
using Android.Graphics;
using Android.Runtime;

namespace AndroidAPI22ADCLibrary.Fragments
{


    public class FragmentSearch : SupportFragment
    {
        private static List<bool> imageChecked = new List<bool>();
        private List<string> notificaciones = new List<string>();
        private SimpleStringRecyclerViewAdapter SSRVA;

        //private Android.Support.V7.Widget.SearchView _searchView;


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        void OnItemClick(object sender, int position)
        {
            Console.WriteLine("Se ha presionado el elemento " + position.ToString() + " del recyclerview");
            Toast.MakeText(this.Activity, "Se ha presionado el elemento " + position.ToString() + " del recyclerview", ToastLength.Short).Show();
            notificaciones.RemoveAt(position);
            if (SSRVA != null)
            {
                SSRVA.NotifyItemRemoved(position);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            RecyclerView recyclerView = inflater.Inflate(Resource.Layout.FragmentRecyclerView, container, false) as RecyclerView;
            SetUpRecyclerView(recyclerView);
            return recyclerView;
        }

        
        private void SetUpRecyclerView(RecyclerView recyclerView)
        {
            notificaciones.Add("Elemento 1");
            notificaciones.Add("Elemento 2");
            notificaciones.Add("Elemento 3");
            notificaciones.Add("Elemento 4");
            notificaciones.Add("Elemento 5");
            notificaciones.Add("Elemento 6");
            notificaciones.Add("Elemento 7");
            notificaciones.Add("Elemento 8");
            notificaciones.Add("Elemento 9");
            notificaciones.Add("Elemento 10");
            notificaciones.Add("Elemento 11");
            notificaciones.Add("Elemento 12");
            notificaciones.Add("Elemento 13");
            notificaciones.Add("Elemento 14");
            notificaciones.Add("Elemento 15");

            recyclerView.SetLayoutManager(new LinearLayoutManager(recyclerView.Context));
            SSRVA = new SimpleStringRecyclerViewAdapter(recyclerView.Context, notificaciones, Activity.Resources);
            SSRVA.ItemClick += OnItemClick;
            recyclerView.SetAdapter(SSRVA);
            //recyclerView.SetOnClickListener()
        }

        

        public static FragmentList newInstance(int queryType)
        {
            FragmentList f = new FragmentList();
            Bundle args = new Bundle();
            args.PutInt("someInt", queryType);

            if (args != null)
            {
                f.Arguments = args;
            }
            else { }
            return f;
        }

        //public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        //{
        //    inflater.Inflate(Resource.Menu.auxMenu, menu);
        //    var item = menu.FindItem(Resource.Id.search);
        //    var searchView = MenuItemCompat.GetActionView(item);
        //    _searchView = searchView.JavaCast<Android.Support.V7.Widget.SearchView>();
        //    _searchView.QueryTextChange += (s, e) =>      
        //    {
        //        //_adapter.Filter.InvokeFilter(e.NewText)
        //        Console.WriteLine("En busqueda: "+e.NewText);
        //    };

        //    base.OnCreateOptionsMenu(menu, inflater);
        //}

        public class SimpleStringRecyclerViewAdapter : RecyclerView.Adapter
        {

            private readonly TypedValue mTypedValue = new TypedValue();
            private int mBackground;
            private List<string> mValues;
            Resources mResource;
            private Dictionary<int, int> mCalculatedSizes;

            public event EventHandler<int> ItemClick;

            //public event EventHandler<int> ItemClick;

            public SimpleStringRecyclerViewAdapter(Context context, List<string> items, Resources res)
            {
                context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, mTypedValue, true);
                mBackground = mTypedValue.ResourceId;
                mValues = items;
                mResource = res;
                mCalculatedSizes = new Dictionary<int, int>();
            }

            //This will fire any event handlers that are registered with our ItemClick
            //event.
            private void OnClick(int position)
            {
                if (ItemClick != null)
                {
                    ItemClick(this, position);
                    //Toast.MakeText(this., "Element Pressed: "+position.ToString(),ToastLength.Short).Show();
                    Console.WriteLine("Element Pressed: " + position.ToString());
                }
            }

            public override int ItemCount
            {
                get
                {
                    return mValues.Count;
                }
            }

            public override async void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {

                var simpleHolder = holder as SimpleViewHolder;

                // En este punto se introducen los datos a mostrar en pantalla.
                simpleHolder.mBoundString = mValues[position];
                simpleHolder.mTxtView.Text = mValues[position];
                try
                {
                    //
                    imageChecked.Add(false);
                    Console.Write("Element " + position.ToString() + " from bool list set to false");
                }
                catch (Exception ex) { Console.WriteLine("--fzeledon: ERROR in bool list: " + ex.ToString()); }

                Console.WriteLine("fzeledon -- Agregando texto de posicion " + position.ToString() + " : " + mValues[position]);

                int drawableID = 0;

                try
                {
                    if (!imageChecked[position])
                    {

                        drawableID = Helper.ImagePicker(position);
                        Console.WriteLine("--fzeledon Drawing not checked element " + position.ToString());
                    }
                    else
                    {
                        Console.WriteLine("--fzeledon Drawing checked element " + position.ToString());
                        drawableID = Resource.Drawable.check;
                    }
                }

                catch (Exception ex) { Console.WriteLine("Error when setting image for element " + position.ToString() + " " + ex.ToString()); }
                BitmapFactory.Options options = new BitmapFactory.Options();
                var bitMap = await BitmapFactory.DecodeResourceAsync(mResource, drawableID, options);
                simpleHolder.mImageView.SetImageBitmap(bitMap);

                //controlador de eventos para las vistas de imagenes circulares.
                simpleHolder.mImageView.Click += delegate
                {
                    Console.WriteLine("fzeledon: element Clicked: " + position.ToString());

                    try
                    {
                        bool check = imageChecked[position];
                        if (!check && (position >= 0))
                        {
                            imageChecked[position] = true;
                            //simpleHolder.mImageView.SetImageBitmap(bitMap);
                            simpleHolder.mImageView.SetImageResource(Resource.Drawable.check);
                            //NotifyItemChanged(position);
                        }
                        else
                        {
                            if (position >= 0)
                            {
                                imageChecked[position] = false;
                                simpleHolder.mImageView.SetImageResource(Helper.ImagePicker(position));
                                //NotifyItemChanged(position);
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine("Error in click event description: " + ex.ToString()); }
                };


            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.List_Item, parent, false);
                view.SetBackgroundResource(mBackground);
                return new SimpleViewHolder(view, OnClick);
            }
        }

        public class SimpleViewHolder : RecyclerView.ViewHolder
        {
            public string mBoundString;
            public readonly View mView;
            public readonly ImageView mImageView;
            public readonly TextView mTxtView;

            public SimpleViewHolder(View view, Action<int> listener) : base(view)
            {
                mView = view;
                mImageView = view.FindViewById<ImageView>(Resource.Id.avatar);
                mTxtView = view.FindViewById<TextView>(Resource.Id.text1);

                //mView.Click += (sender, e) =>
                //{                 
                //    int position = Position;
                //    Console.WriteLine("Click en el recyclerview en la posicion "+position.ToString());
                //    //var context = mView.Context;
                //    //var intent = new Intent(context, typeof(DetailActivity));
                //    //context.StartActivity(intent);
                //};

                view.Click += (sender, e) => listener(Position);
            }

            public override string ToString()
            {
                return base.ToString() + " '" + mTxtView.Text;
            }

        }
    }
}