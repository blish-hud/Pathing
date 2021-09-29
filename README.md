# 📦 Pathing Module [bh.community.pathing]
[![Discord](https://img.shields.io/discord/531175899588984842.svg?logo=discord&logoColor=%237289DA)](https://discord.gg/HzAV82d)

The Pathing module is a [Blish HUD](https://blishhud.com/) module which provides support for TacO style marker packs.

- [Review our user documentation](https://blishhud.com/docs/markers/)
- [Review our attribute documentation](https://blishhud.com/docs/markers/attributes/achievement)

## Download the Pathing Module

You can downlaod the Pathing module from:
- Our [Releases](https://github.com/blish-hud/Pathing/releases) page here on GitHub.
- The Blish HUD repository while in-game.

In any case, you can review the Blish HUD [module install guide](https://blishhud.com/docs/user/installing-modules) for more details.

## For Contributors

Pull requests are welcome.  You are encouraged to join the discussion in the [Blish HUD #📦module_dev_discussion Discord channel](https://discord.gg/HzAV82d) or discuss with us through a [submitted issue](https://github.com/blish-hud/Pathing/issues/new).

### Building the Pathing Module

#### Instructions

1. Clone the repo: `git clone https://github.com/blish-hud/Pathing.git`
2. Clone TmfLib: `git clone https://github.com/dlamkins/TmfLib.git`
3. Launch the Pathing solution and ensure that the project reference to TmfLib is properly restored.
4. Follow the standard [debugging steps for Blish HUD modules](https://blishhud.com/docs/modules/overview/debugging).

You can ignore the `Pathing.Harness` project.  It is not necessary for building the Pathing module.

#### TmfLib

TmfLib is a library dedicated to reading and writing Blish HUD / TacO format marker packs.  This library is seperated so that it can be used with other projects as well while ensuring we maintain the same parsing spec.  It's currently also used by our linter and a marker pack re-packer.

### License

Licensed under the [MIT License](https://choosealicense.com/licenses/mit/)