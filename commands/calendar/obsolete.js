const { SlashCommandBuilder } = require('discord.js');
const { Level } = require('level');

module.exports = {
  data: new SlashCommandBuilder()
  .setName('obsolete')
  .setDescription('Delete all obsolete tasks'),
  
	async execute(interaction) {
    const db = new Level('db', { valueEncoding: 'json' })
    const entries = await db.iterator().all()
    if (entries.length == 0) return interaction.reply({ content: "You have nothing to delete", ephemeral: true });

    for (const [key, value] of entries) {    
      if (Date.parse(value.end) < Date.now() - 3 * 24 * 60 * 60 * 1000)
      {  
        await db.del(key)
      } 
    }

    db.close()
    return interaction.reply({ content: "All obsolete tasks deleted" , ephemeral: true });
	},
};