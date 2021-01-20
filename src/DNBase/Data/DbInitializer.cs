using DNBase.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DNBase.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(DNBaseContext context)
        {
            await context.Database.EnsureCreatedAsync();

            // Look for subgenres.
            if (context.Subgenres.Any())
            {
                return; //DB has been seeded.
            }

            var subgenres = new[]
            {
                new SubgenreModel
                {
                    SubgenreName = "Liquid",
                    Artists = new List<ArtistModel>
                    {
                        new ArtistModel
                        {
                            ArtistName = "Hybrid Minds",
                            CountryOfOrigin = "UK",
                            Albums = new List<AlbumModel>
                            {
                                new AlbumModel
                                {
                                    AlbumName = "Mountains",
                                    Tracks = new List<TrackModel>
                                    {
                                        new TrackModel
                                        {
                                            TrackName = "Halcyon"
                                        }
                                    }
                                },

                                new AlbumModel
                                {
                                    AlbumName = "Elements",
                                    Tracks = new List<TrackModel>
                                    {
                                        new TrackModel
                                        {
                                            TrackName = "Touch"
                                        },
                                        new TrackModel
                                        {
                                            TrackName = "Demons"
                                        }
                                    }
                                }
                            }
                        },
                        new ArtistModel
                        {
                            ArtistName = "Amelsis",
                            CountryOfOrigin = "Canada",
                            Albums = new List<AlbumModel>
                            {
                                new AlbumModel
                                {
                                    AlbumName = "Singles",
                                    Tracks = new List<TrackModel>
                                    {
                                        new TrackModel
                                        {
                                            TrackName = "You're Fire"
                                        }
                                    }
                                }
                            }
                        },
                        new ArtistModel
                        {
                            ArtistName = "Lenzman",
                            CountryOfOrigin = "Netherlands",
                            Albums = new List<AlbumModel>
                            {
                                new AlbumModel
                                {
                                    AlbumName = "Earth Tones",
                                    Tracks = new List<TrackModel>
                                    {
                                        new TrackModel
                                        {
                                            TrackName = "In My Mind"
                                        },
                                        new TrackModel
                                        {
                                            TrackName = "Waves"
                                        },
                                        new TrackModel
                                        {
                                            TrackName = "Walk On By"
                                        }
                                    }
                                }
                            }
                        },
                        new ArtistModel
                        {
                            ArtistName = "Monrroe",
                            CountryOfOrigin = "UK",
                            Albums = new List<AlbumModel>
                            {
                                new AlbumModel
                                {
                                    AlbumName = "Endless Change",
                                    Tracks = new List<TrackModel>
                                    {
                                        new TrackModel
                                        {
                                            TrackName = "Horizon"
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                new SubgenreModel
                {
                    SubgenreName = "Jump Up",
                    Artists = new List<ArtistModel>
                    {
                        new ArtistModel
                        {
                            ArtistName = "Dj Hazard",
                            CountryOfOrigin = "UK",
                            Albums = new List<AlbumModel>
                            {
                                new AlbumModel
                                {
                                    AlbumName = "Machete Bass",
                                    Tracks = new List<TrackModel>
                                    {
                                        new TrackModel
                                        {
                                            TrackName = "Killers Don't Die"
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                new SubgenreModel
                {
                    SubgenreName = "Neuro Funk",
                    Artists = new List<ArtistModel>
                    {
                        new ArtistModel
                        {
                            ArtistName = "Noisia",
                            CountryOfOrigin = "Netherlands",
                            Albums = new List<AlbumModel>
                            {
                                new AlbumModel
                                {
                                    AlbumName = "Outer Edges",
                                    Tracks = new List<TrackModel>
                                    {
                                        new TrackModel
                                        {
                                            TrackName = "The Approach"
                                        }
                                    }
                                },
                                new AlbumModel
                                {
                                    AlbumName = "Split The Atoms",
                                    Tracks = new List<TrackModel>
                                    {
                                        new TrackModel
                                        {
                                            TrackName = "Thursday"
                                        },
                                        new TrackModel
                                        {
                                            TrackName = "Shellshock"
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                new SubgenreModel
                {
                    SubgenreName = "Minimal",
                    Artists = new List<ArtistModel>
                    {
                        new ArtistModel
                        {
                            ArtistName = "Alix Perez",
                            CountryOfOrigin = "Belgium",
                            Albums = new List<AlbumModel>
                            {
                                new AlbumModel
                                {
                                    AlbumName = "1984",
                                    Tracks = new List<TrackModel>
                                    {
                                        new TrackModel
                                        {
                                            TrackName = "Fade Away"
                                        },
                                        new TrackModel
                                        {
                                            TrackName = "Forsaken"
                                        }
                                    }
                                },

                                new AlbumModel
                                {
                                    AlbumName = "Phantonym EP",
                                    Tracks = new List<TrackModel>
                                    {
                                        new TrackModel
                                        {
                                            TrackName = "Trinity"
                                        },
                                        new TrackModel
                                        {
                                            TrackName = "Phantonym"
                                        },
                                        new TrackModel
                                        {
                                            TrackName = "SWRV"
                                        },
                                        new TrackModel
                                        {
                                            TrackName = "Vibrations"
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                new SubgenreModel
                {
                    SubgenreName = "Drumstep",
                    Artists = new List<ArtistModel>
                    {
                        new ArtistModel
                        {
                            ArtistName = "Sub Focus",
                            CountryOfOrigin = "UK",
                            Albums = new List<AlbumModel>
                            {
                                new AlbumModel
                                {
                                    AlbumName = "Torus",
                                    Tracks = new List<TrackModel>
                                    {
                                        new TrackModel
                                        {
                                            TrackName = "Twilight"
                                        },
                                        new TrackModel
                                        {
                                            TrackName = "Tidal Wave"
                                        }
                                    }
                                }
                            }
                        },
                        new ArtistModel
                        {
                            ArtistName = "Camo & Krooked",
                            CountryOfOrigin = "Austria",
                            Albums = new List<AlbumModel>
                            {
                                new AlbumModel
                                {
                                    AlbumName = "Mosaik",
                                    Tracks = new List<TrackModel>
                                    {
                                        new TrackModel
                                        {
                                            TrackName = "Good Times Bad Times"
                                        },
                                        new TrackModel
                                        {
                                            TrackName = "Ember"
                                        },
                                        new TrackModel
                                        {
                                            TrackName = "Heat Of The Moment"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            await context.Subgenres.AddRangeAsync(subgenres);
            await context.SaveChangesAsync();
        }
    }
}
