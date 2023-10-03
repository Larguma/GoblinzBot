const { SlashCommandBuilder } = require('discord.js');

module.exports = {
  data: new SlashCommandBuilder()
    .setName('time')
    .setDescription('Show timetable'),
	async execute(interaction) {
    let out = "## Timetable ISC-IL-2a 2023-2024 Fall\n"
    out += "```| Time        | Mon               | Tue      | Wed         | Thu          | Fri       |\n"
    out +=    "| ----------- | ----------------- | -------- | ----------- | ------------ | --------- |\n"
    out +=    "| 08:15-09:00 | Algo              | SysInfo  | Physique    | Gestion proj | Admin     |\n"
    out +=    "|             | D2012             | D2012    | B3020       | D2012        | D2012     |\n"
    out +=    "| 09:05-09:50 |                   |          |             |              | Arch Ordi |\n"
    out +=    "|             |                   |          |             |              | D2012     |\n"
    out +=    "| 10:15-11:00 |                   | Physique | Maths       | Stats        |           |\n"
    out +=    "|             |                   | B3004    | D2012       | D2012        |           |\n"
    out +=    "| 11:05-11:50 |                   |          |             |              |           |\n"
    out +=    "| =========== | ================= | ======== | =========== | ============ | ========= |\n"
    out +=    "| 13:00-13:45 | Concurp/Arch ordi | Concurp  | DevOps      | App mobile   | Maths     |\n"
    out +=    "|             | D2012/C0016       | D2012    | D2012/C0022 | D2012        | D2012     |\n"
    out +=    "| 13:50-14:35 |                   |          |             |              |           |\n"
    out +=    "| 15:00-15:45 |                   |          | =========== |              | ========= |\n"
    out +=    "| 15:50-16:35 |                   | ======== | =========== |              | ========= |```"
    return interaction.reply({ content: out, ephemeral: false });
	},
};