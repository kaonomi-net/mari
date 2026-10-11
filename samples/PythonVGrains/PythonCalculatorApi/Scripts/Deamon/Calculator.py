import re
import sys
import os
import mari

def calc(command: mari.vgrainCommand, tools: mari.vgrainTools):
    """ Eval and calculate the mathematical result """
    formula = command.get_content()
    result = eval(formula)
    tools.get_logger.logInformation("Test Result " + formula + "=" + str(result))
    return result

app = mari.vgrain(sys.argv)

app.run(calc);
