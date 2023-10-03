const { SlashCommandBuilder } = require('discord.js');
const { Level } = require('level');

module.exports = {
  data: new SlashCommandBuilder()
  .setName('list')
  .setDescription('List tasks')
  .addStringOption(option =>
		option.setName('career')
			.setDescription('Who ?')
			.setRequired(true)
			.addChoices(
				{ name: 'IL', value: 'IL' },
        { name: 'RS', value: 'RS' },
			)),
	async execute(interaction) {
    const career = interaction.options.getString('career');

    const db = new Level('db', { valueEncoding: 'json' })
    const entries = await db.iterator().all()
    db.close()
    if (entries.length == 0) return interaction.reply({ content: "You have nothing to do", ephemeral: true });
    let items = []
    for (const [key, value] of entries) {
      items.push(value);
    }
    
    let out = "## " + career + "\n```diff\n";
    for (const value of items.sort((x,y) => x.end > y.end ? 1 : -1)) {      
      if (career == value.career || value.career == "BOTH")
      {
        if (Date.parse(value.end) > Date.now() - 3 * 24 * 60 * 60 * 1000)
        {
          if (Date.parse(value.end) < Date.now()) out += "- ";
          else if (value.exa == "true") out += "+ ";
          else out += "  ";
          out += value.end + " - " + value.lesson + ": " + value.title + "\n";
        }
      }

    }
    out += "```"

    return interaction.reply({ content: out, ephemeral: false });
	},
};