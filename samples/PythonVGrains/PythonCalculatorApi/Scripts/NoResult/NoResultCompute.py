import sys
import os
import mari

def noCompute(command: mari.vgrainCommand, tools: mari.vgrainTools):
    """ Eval and compute the mathematical result """
    formula = command.get_content()
    result = eval(formula)
    tools.get_logger.logInformation("Test Result " + formula + "=" + str(result))

app = mari.vgrain(sys.argv)

app.execute(noCompute);
